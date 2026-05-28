"use client"

import { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from "react"
import { api, setOnSessionExpired } from "./api"

export interface AuthUser {
  userId: string
  email: string
  name: string
  role: "Admin" | "Instructor" | "Trainee"
  organizationId: string
}

interface AuthContextType {
  user: AuthUser | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string, name: string, organizationId: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    setOnSessionExpired(() => { setUser(null) })

    const stored = localStorage.getItem("user")
    if (stored) {
      try { setUser(JSON.parse(stored)) } catch { /* ignore */ }
    }

    const validateSession = async () => {
      if (!localStorage.getItem("accessToken")) {
        setLoading(false)
        return
      }
      try {
        const data = await api.get<{
          userId: string
          email: string
          name: string
          role: string
          organizationId: string
        }>("/api/auth/me")

        const validatedUser: AuthUser = {
          userId: data.userId,
          email: data.email,
          name: data.name,
          role: data.role as AuthUser["role"],
          organizationId: data.organizationId,
        }
        setUser(validatedUser)
        localStorage.setItem("user", JSON.stringify(validatedUser))
      } catch {
        localStorage.removeItem("accessToken")
        localStorage.removeItem("refreshToken")
        localStorage.removeItem("user")
        setUser(null)
        window.location.href = "/login"
      } finally {
        setLoading(false)
      }
    }

    validateSession()
    return () => { setOnSessionExpired(() => {}) }
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const res = await api.post<{
      accessToken: string
      refreshToken: string
      userId: string
      email: string
      name: string
      role: string
      organizationId: string
    }>("/api/auth/login", { email, password })

    localStorage.setItem("accessToken", res.accessToken)
    localStorage.setItem("refreshToken", res.refreshToken)
    const userData: AuthUser = {
      userId: res.userId,
      email: res.email,
      name: res.name,
      role: res.role as AuthUser["role"],
      organizationId: res.organizationId,
    }
    localStorage.setItem("user", JSON.stringify(userData))
    setUser(userData)
  }, [])

  const register = useCallback(async (email: string, password: string, name: string, organizationId: string) => {
    const res = await api.post<{
      accessToken: string
      refreshToken: string
      userId: string
      email: string
      name: string
      role: string
      organizationId: string
    }>("/api/auth/register", { email, password, name, role: "Admin", organizationId })

    localStorage.setItem("accessToken", res.accessToken)
    localStorage.setItem("refreshToken", res.refreshToken)
    const userData: AuthUser = {
      userId: res.userId,
      email: res.email,
      name: res.name,
      role: res.role as AuthUser["role"],
      organizationId: res.organizationId,
    }
    localStorage.setItem("user", JSON.stringify(userData))
    setUser(userData)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem("accessToken")
    localStorage.removeItem("refreshToken")
    localStorage.removeItem("user")
    setUser(null)
  }, [])

  return (
    <AuthContext.Provider value={{ user, loading, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error("useAuth must be used within AuthProvider")
  return ctx
}
