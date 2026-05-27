import { describe, it, expect, vi, beforeEach, afterEach } from "vitest"

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Set localStorage to a known token state. */
function setToken(accessToken?: string, refreshToken?: string) {
  if (accessToken) localStorage.setItem("accessToken", accessToken)
  else localStorage.removeItem("accessToken")

  if (refreshToken) localStorage.setItem("refreshToken", refreshToken)
  else localStorage.removeItem("refreshToken")
}

/** Create a minimal Response-compatible object. */
function mockResponse(overrides: Partial<Response> = {}): Response {
  const ok = overrides.status !== undefined ? overrides.status >= 200 && overrides.status < 300 : true
  return {
    ok,
    status: overrides.status ?? 200,
    statusText: overrides.statusText ?? "OK",
    headers: new Headers(),
    redirected: false,
    type: "basic" as ResponseType,
    url: "",
    // @ts-expect-error - partial mock of Response
    json: overrides.json ?? (() => Promise.resolve({})),
    // @ts-expect-error - partial mock of Response
    text: overrides.text ?? (() => Promise.resolve("")),
    clone: function () { return mockResponse(overrides) },
    ...overrides,
  } as Response
}

// ---------------------------------------------------------------------------
// Suite
// ---------------------------------------------------------------------------

describe("api client", () => {
  const mockFetch = vi.fn<typeof fetch>()

  beforeEach(() => {
    vi.stubGlobal("fetch", mockFetch)
    localStorage.clear()
    mockFetch.mockReset()
    // Prevent actual navigation during tests
    Object.defineProperty(window, "location", {
      value: { href: "http://localhost" },
      writable: true,
    })
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  // ---- Auth header injection -----------------------------------------------

  it("adds Authorization Bearer header when token is in localStorage", async () => {
    setToken("my-access-token")
    mockFetch.mockResolvedValue(mockResponse({ status: 200, json: () => Promise.resolve({ data: "ok" }) }))

    const { api } = await import("../api")

    await api.get("/some/path")

    expect(mockFetch).toHaveBeenCalledTimes(1)
    const [, opts] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(opts.headers).toMatchObject({ Authorization: "Bearer my-access-token" })
  })

  it("adds auth header on POST, PUT, DELETE", async () => {
    setToken("token-p")
    mockFetch.mockResolvedValue(mockResponse({ status: 200, json: () => Promise.resolve({}) }))

    const { api } = await import("../api")

    await api.post("/post", { a: 1 })
    await api.put("/put", { b: 2 })
    await api.delete("/delete")

    for (const [, opts] of mockFetch.mock.calls as [string, RequestInit][]) {
      expect(opts.headers).toMatchObject({ Authorization: "Bearer token-p" })
    }
  })

  it("does NOT add auth header when no token is stored", async () => {
    localStorage.clear()
    mockFetch.mockResolvedValue(mockResponse({ status: 200, json: () => Promise.resolve({}) }))

    const { api } = await import("../api")
    await api.get("/no-auth")

    const [, opts] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(opts.headers).not.toHaveProperty("Authorization")
  })

  // ---- 401 + token refresh -------------------------------------------------

  it("triggers token refresh on 401 and retries the original request", async () => {
    setToken("expired-token", "valid-refresh")

    let callIndex = 0
    mockFetch.mockImplementation(async (url: string) => {
      callIndex++
      if (callIndex === 1) {
        // Original request returns 401
        return mockResponse({ status: 401 })
      }
      if (callIndex === 2) {
        // Refresh endpoint returns new tokens
        return mockResponse({
          status: 200,
          json: () => Promise.resolve({
            accessToken: "new-access",
            refreshToken: "new-refresh",
            userId: "u1",
            email: "a@b.com",
            name: "Test",
            role: "Admin",
            organizationId: "org1",
          }),
        })
      }
      // Retry of original request succeeds
      return mockResponse({ status: 200, json: () => Promise.resolve({ data: "retried" }) })
    })

    const { api } = await import("../api")
    const result = await api.get("/needs-refresh")
    expect(result).toEqual({ data: "retried" })
    // 3 calls: original GET (401) → POST refresh → retried GET
    expect(mockFetch).toHaveBeenCalledTimes(3)
  })

  it("queues concurrent 401s and only runs one refresh", async () => {
    setToken("expired", "refresh-token")

    let refreshAttempts = 0
    // Track which URLs have been retried already
    const retried = new Set<string>()

    mockFetch.mockImplementation(async (url: string, opts?: RequestInit) => {
      if (url.toString().includes("/api/auth/refresh")) {
        refreshAttempts++
        return mockResponse({
          status: 200,
          json: () => Promise.resolve({
            accessToken: "new-token",
            refreshToken: "new-refresh",
            userId: "u1",
            email: "a@b.com",
            name: "Test",
            role: "Admin",
            organizationId: "org1",
          }),
        })
      }

      // Return 401 only on first attempt (not on retry)
      if (retried.has(url.toString())) {
        return mockResponse({ status: 200, json: () => Promise.resolve({ ok: true }) })
      }
      retried.add(url.toString())
      return mockResponse({ status: 401 })
    })

    const { api } = await import("../api")

    // Fire three concurrent requests — only ONE refresh should happen
    const results = await Promise.allSettled([
      api.get("/a"),
      api.get("/b"),
      api.get("/c"),
    ])

    expect(refreshAttempts).toBe(1)
    expect(results).toHaveLength(3)
    // All should have resolved successfully after retry
    for (const r of results) {
      expect(r.status).toBe("fulfilled")
    }
  })

  it("clears auth state and calls onSessionExpired when refresh fails", async () => {
    setToken("expired", "bad-refresh-token")
    const onExpired = vi.fn()

    const { setOnSessionExpired, api } = await import("../api")
    setOnSessionExpired(onExpired)

    // All requests return 401 (including refresh)
    mockFetch.mockResolvedValue(mockResponse({ status: 401 }))

    await expect(api.get("/fail")).rejects.toThrow("Session expired")

    // Auth state should be cleared
    expect(localStorage.getItem("accessToken")).toBeNull()
    expect(localStorage.getItem("refreshToken")).toBeNull()
    expect(localStorage.getItem("user")).toBeNull()
    // Callback should have fired
    expect(onExpired).toHaveBeenCalledTimes(1)
  })

  // ---- 204 handling --------------------------------------------------------

  it("returns undefined when response status is 204", async () => {
    setToken("tok")
    mockFetch.mockResolvedValue(mockResponse({ status: 204 }))

    const { api } = await import("../api")
    const result = await api.get("/no-content")
    expect(result).toBeUndefined()
  })

  // ---- Error handling ------------------------------------------------------

  it("throws on non-OK response with body text", async () => {
    setToken("tok")
    mockFetch.mockResolvedValue(
      mockResponse({ status: 400, text: () => Promise.resolve("Bad request body") }),
    )

    const { api } = await import("../api")
    await expect(api.get("/bad")).rejects.toThrow("Bad request body")
  })

  it("throws a generic message when non-OK response has no body", async () => {
    setToken("tok")
    mockFetch.mockResolvedValue(
      mockResponse({ status: 500, text: () => Promise.resolve("") }),
    )

    const { api } = await import("../api")
    await expect(api.get("/error")).rejects.toThrow("Request failed: 500")
  })

  // ---- setOnSessionExpired -------------------------------------------------

  it("fires onSessionExpired callback when refresh fails repeatedly", async () => {
    setToken("exp", "bad-refresh")
    const spy = vi.fn()

    const { setOnSessionExpired, api } = await import("../api")
    setOnSessionExpired(spy)

    mockFetch.mockResolvedValue(mockResponse({ status: 401 }))

    await expect(api.get("/")).rejects.toThrow()
    expect(spy).toHaveBeenCalledTimes(1)
  })
})
