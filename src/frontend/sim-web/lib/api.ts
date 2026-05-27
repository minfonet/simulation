const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:8080"

interface RequestOptions {
  method?: string
  body?: unknown
  headers?: Record<string, string>
}

// Token refresh state
let isRefreshing = false
type RetryCallback = (newToken: string) => void
let pendingRequests: RetryCallback[] = []
let onSessionExpiredCallback: (() => void) | null = null

/**
 * Register a callback that fires when the session expires
 * (refresh token also invalid). Used by AuthProvider to clear user state.
 */
export function setOnSessionExpired(callback: () => void) {
  onSessionExpiredCallback = callback
}

/**
 * Attempt to refresh the access token using the stored refresh token.
 * Returns true on success, false on failure.
 */
export async function refreshAuth(): Promise<boolean> {
  const refreshToken =
    typeof window !== "undefined" ? localStorage.getItem("refreshToken") : null
  if (!refreshToken) return false

  try {
    const res = await fetch(`${API_URL}/api/auth/refresh`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    })

    if (!res.ok) return false

    const data = await res.json()
    localStorage.setItem("accessToken", data.accessToken)
    localStorage.setItem("refreshToken", data.refreshToken)
    if (data.userId && data.email && data.name && data.role) {
      localStorage.setItem("user", JSON.stringify({
        userId: data.userId,
        email: data.email,
        name: data.name,
        role: data.role,
        organizationId: data.organizationId,
      }))
    }
    return true
  } catch {
    return false
  }
}

function clearAuthState() {
  localStorage.removeItem("accessToken")
  localStorage.removeItem("refreshToken")
  localStorage.removeItem("user")
  onSessionExpiredCallback?.()
  onSessionExpiredCallback = null
  if (typeof window !== "undefined") {
    window.location.href = "/login"
  }
}

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, headers = {} } = options

  const token =
    typeof window !== "undefined" ? localStorage.getItem("accessToken") : null

  const res = await fetch(`${API_URL}${path}`, {
    method,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body: body ? JSON.stringify(body) : undefined,
  })

  // Handle 401 — attempt token refresh, or queue if already refreshing
  if (res.status === 401 && path !== "/api/auth/refresh") {
    if (isRefreshing) {
      // Another refresh is in flight — queue this request to retry later
      return new Promise<T>((resolve, reject) => {
        pendingRequests.push((newToken: string) => {
          request<T>(path, {
            ...options,
            headers: { ...headers, Authorization: `Bearer ${newToken}` },
          })
            .then(resolve)
            .catch(reject)
        })
      })
    }

    // First 401 — start the refresh flow
    isRefreshing = true

    try {
      if (!(await refreshAuth())) throw new Error("Refresh failed")

      const newToken = localStorage.getItem("accessToken")
      if (!newToken) throw new Error("Token lost")

      const queue = [...pendingRequests]
      pendingRequests = []
      queue.forEach((cb) => cb(newToken))
      isRefreshing = false

      return request<T>(path, {
        ...options,
        headers: { ...headers, Authorization: `Bearer ${newToken}` },
      })
    } catch {
      pendingRequests = []
      isRefreshing = false
      clearAuthState()
      throw new Error("Session expired")
    }
  }

  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `Request failed: ${res.status}`)
  }

  if (res.status === 204) return undefined as T
  return res.json()
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body: unknown) => request<T>(path, { method: "POST", body }),
  put: <T>(path: string, body: unknown) => request<T>(path, { method: "PUT", body }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
}
