import { describe, it, expect, vi, beforeEach, afterEach } from "vitest"
import { render, screen, waitFor, act } from "@testing-library/react"
import React from "react"

// ---------------------------------------------------------------------------
// Mocks — vi.mock is hoisted, so use vi.hoisted() for factory variables
// ---------------------------------------------------------------------------

const { mockGet, mockPost, mockSetOnSessionExpired } = vi.hoisted(() => ({
  mockGet: vi.fn(),
  mockPost: vi.fn(),
  mockSetOnSessionExpired: vi.fn(),
}))

vi.mock("../api", () => ({
  api: {
    get: mockGet,
    post: mockPost,
    put: vi.fn(),
    delete: vi.fn(),
  },
  setOnSessionExpired: mockSetOnSessionExpired,
}))

// ---------------------------------------------------------------------------
// Suite
// ---------------------------------------------------------------------------

describe("AuthProvider", () => {
  beforeEach(() => {
    localStorage.clear()
    mockGet.mockReset()
    mockPost.mockReset()
    mockSetOnSessionExpired.mockReset()

    // Prevent actual navigation during tests
    Object.defineProperty(window, "location", {
      value: { href: "http://localhost" },
      writable: true,
    })
  })

  afterEach(() => {
    vi.clearAllMocks()
  })

  async function loadAuth() {
    return import("../auth-context")
  }

  // ---- Loading state --------------------------------------------------------

  it("shows loading state initially when a token exists", async () => {
    localStorage.setItem("accessToken", "some-token")
    localStorage.setItem("user", JSON.stringify({
      userId: "u1",
      email: "a@b.com",
      name: "A",
      role: "Admin",
      organizationId: "org1",
    }))

    // Prevent /api/auth/me from resolving so loading stays true
    mockGet.mockImplementation(() => new Promise(() => {}))

    const { AuthProvider, useAuth } = await loadAuth()

    function Consumer() {
      const auth = useAuth()
      return React.createElement("span", { "data-testid": "loading-val" }, auth.loading ? "loading" : "done")
    }

    render(React.createElement(AuthProvider, null, React.createElement(Consumer)))

    expect(screen.getByTestId("loading-val").textContent).toBe("loading")
  })

  // ---- Restore user from localStorage ---------------------------------------

  it("restores user from localStorage on mount", async () => {
    const userData = { userId: "u1", email: "a@b.com", name: "Alice", role: "Admin" as const, organizationId: "org1" }
    localStorage.setItem("user", JSON.stringify(userData))
    localStorage.setItem("accessToken", "tok")

    mockGet.mockResolvedValue({
      userId: "u1",
      email: "a@b.com",
      name: "Alice",
      role: "Admin",
      organizationId: "org1",
    })

    const { AuthProvider, useAuth } = await loadAuth()

    function Consumer() {
      const auth = useAuth()
      return React.createElement("span", { "data-testid": "user-name" }, auth.user?.name ?? "none")
    }

    render(React.createElement(AuthProvider, null, React.createElement(Consumer)))

    await waitFor(() => {
      expect(screen.getByTestId("user-name").textContent).toBe("Alice")
    })
  })

  // ---- login() --------------------------------------------------------------

  it("login() stores tokens and user data to localStorage", async () => {
    mockPost.mockResolvedValue({
      accessToken: "at1",
      refreshToken: "rt1",
      userId: "u2",
      email: "b@b.com",
      name: "Bob",
      role: "Instructor",
      organizationId: "org2",
    })

    const { AuthProvider, useAuth } = await loadAuth()

    function LoginHarness() {
      const auth = useAuth()
      return React.createElement("button", {
        "data-testid": "login-btn",
        onClick: () => auth.login("b@b.com", "secret"),
      }, "Login")
    }

    render(React.createElement(AuthProvider, null, React.createElement(LoginHarness)))

    await act(async () => {
      screen.getByTestId("login-btn").click()
    })

    expect(localStorage.getItem("accessToken")).toBe("at1")
    expect(localStorage.getItem("refreshToken")).toBe("rt1")
    const storedUser = JSON.parse(localStorage.getItem("user")!)
    expect(storedUser.name).toBe("Bob")
    expect(storedUser.role).toBe("Instructor")
  })

  // ---- logout() -------------------------------------------------------------

  it("logout() clears localStorage", async () => {
    localStorage.setItem("accessToken", "at")
    localStorage.setItem("refreshToken", "rt")
    localStorage.setItem("user", JSON.stringify({ name: "X" }))

    const { AuthProvider, useAuth } = await loadAuth()

    function LogoutHarness() {
      const auth = useAuth()
      return React.createElement("button", {
        "data-testid": "logout-btn",
        onClick: () => auth.logout(),
      }, "Logout")
    }

    render(React.createElement(AuthProvider, null, React.createElement(LogoutHarness)))

    await act(async () => {
      screen.getByTestId("logout-btn").click()
    })

    expect(localStorage.getItem("accessToken")).toBeNull()
    expect(localStorage.getItem("refreshToken")).toBeNull()
    expect(localStorage.getItem("user")).toBeNull()
  })

  // ---- validateSession on mount ---------------------------------------------

  it("calls GET /api/auth/me on mount when accessToken exists", async () => {
    localStorage.setItem("accessToken", "tok")
    localStorage.setItem("user", JSON.stringify({
      userId: "u1", email: "a@b.com", name: "A", role: "Admin", organizationId: "org1",
    }))

    mockGet.mockResolvedValue({
      userId: "u1",
      email: "a@b.com",
      name: "A",
      role: "Admin",
      organizationId: "org1",
    })

    const { AuthProvider } = await loadAuth()
    render(React.createElement(AuthProvider, null, React.createElement("div", null, "rendered")))

    await waitFor(() => {
      expect(mockGet).toHaveBeenCalledWith("/api/auth/me")
    })
  })

  // ---- validateSession failure ----------------------------------------------

  it("redirects to /login when validateSession fails", async () => {
    localStorage.setItem("accessToken", "bad-token")
    localStorage.setItem("refreshToken", "bad-refresh")
    localStorage.setItem("user", JSON.stringify({
      userId: "u1", email: "a@b.com", name: "A", role: "Admin", organizationId: "org1",
    }))

    mockGet.mockRejectedValue(new Error("Unauthorized"))

    const { AuthProvider } = await loadAuth()
    render(React.createElement(AuthProvider, null, React.createElement("div", null, "content")))

    await waitFor(() => {
      expect(window.location.href).toBe("/login")
    })

    expect(localStorage.getItem("accessToken")).toBeNull()
    expect(localStorage.getItem("refreshToken")).toBeNull()
    expect(localStorage.getItem("user")).toBeNull()
  })

  // ---- setOnSessionExpired registration -------------------------------------

  it("registers setOnSessionExpired callback on mount", async () => {
    const { AuthProvider } = await loadAuth()
    render(React.createElement(AuthProvider, null, React.createElement("div", null, "content")))

    await waitFor(() => {
      expect(mockSetOnSessionExpired).toHaveBeenCalled()
    })
  })
})
