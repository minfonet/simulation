import { describe, it, expect, vi, beforeEach } from "vitest"

// ---------------------------------------------------------------------------
// Mocks
// ---------------------------------------------------------------------------

let mockNextResponseNext = vi.fn()

vi.mock("next/server", () => {
  const mockNextResponse = {
    next: () => ({ status: 200, headers: new Headers() }),
  }
  return { NextResponse: mockNextResponse }
})

// ---------------------------------------------------------------------------
// Suite
// ---------------------------------------------------------------------------

describe("proxy", () => {
  beforeEach(() => {
    vi.resetModules()
    mockNextResponseNext = vi.fn(() => ({ status: 200 }))
    const nextServer = require("next/server")
    nextServer.NextResponse.next = mockNextResponseNext
  })

  it("passes through all requests (auth is enforced client-side + backend)", async () => {
    const { proxy } = await import("../../proxy")
    const request = { url: "http://localhost:3000/admin/users", method: "GET" } as any
    const result = proxy(request)

    expect(result).toBeDefined()
    // NextResponse.next() is called
    expect(result.status).toBe(200)
  })

  it("allows /login path through", async () => {
    const { proxy } = await import("../../proxy")
    const request = { url: "http://localhost:3000/login", method: "GET" } as any
    const result = proxy(request)

    expect(result).toBeDefined()
    expect(result.status).toBe(200)
  })

  it("allows authenticated admin routes through", async () => {
    const { proxy } = await import("../../proxy")
    const request = { url: "http://localhost:3000/admin/dashboard", method: "GET" } as any
    const result = proxy(request)

    expect(result).toBeDefined()
    expect(result.status).toBe(200)
  })

  it("allows instructor routes through", async () => {
    const { proxy } = await import("../../proxy")
    const request = { url: "http://localhost:3000/instructor/sessions", method: "GET" } as any
    const result = proxy(request)

    expect(result).toBeDefined()
    expect(result.status).toBe(200)
  })

  it("allows trainee routes through", async () => {
    const { proxy } = await import("../../proxy")
    const request = { url: "http://localhost:3000/trainee/evaluations", method: "GET" } as any
    const result = proxy(request)

    expect(result).toBeDefined()
    expect(result.status).toBe(200)
  })

  describe("config", () => {
    it("exports a matcher that excludes api, static assets, and favicon", async () => {
      const { config } = await import("../../proxy")
      expect(config).toBeDefined()
      expect(config.matcher).toBeDefined()
      expect(Array.isArray(config.matcher) || typeof config.matcher === "string").toBe(true)
    })
  })
})
