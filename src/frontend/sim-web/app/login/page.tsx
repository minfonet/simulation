"use client"

import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAuth } from "@/lib/auth-context"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card"

const roleRedirect: Record<string, string> = {
  Admin: "/admin",
  Instructor: "/instructor",
  Trainee: "/trainee",
}

const BOOTSTRAP_ORG_ID = "00000000-0000-0000-0000-000000000001"

export default function LoginPage() {
  const { user, login, register, loading } = useAuth()
  const router = useRouter()
  const [isSignup, setIsSignup] = useState(false)
  const [email, setEmail] = useState("")
  const [password, setPassword] = useState("")
  const [name, setName] = useState("")
  const [error, setError] = useState("")
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (user && !loading) {
      router.replace(roleRedirect[user.role] ?? "/login")
    }
  }, [user, loading, router])

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError("")
    setSubmitting(true)
    try {
      if (isSignup) {
        await register(email, password, name, BOOTSTRAP_ORG_ID)
      } else {
        await login(email, password)
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : isSignup ? "Registration failed" : "Login failed")
    } finally {
      setSubmitting(false)
    }
  }

  function toggleMode() {
    setIsSignup((prev) => !prev)
    setError("")
  }

  if (loading) return null

  return (
    <div className="flex min-h-screen items-center justify-center bg-zinc-50">
      <Card className="w-full max-w-sm">
        <CardHeader className="text-center">
          <CardTitle>{isSignup ? "Create Account" : "Simulation Platform"}</CardTitle>
          <CardDescription>{isSignup ? "Register as an Admin" : "Sign in to your account"}</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            {isSignup && (
              <div className="space-y-2">
                <Label htmlFor="name">Name</Label>
                <Input
                  id="name"
                  type="text"
                  placeholder="Your full name"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  required
                />
              </div>
            )}
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="you@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                type="password"
                placeholder="••••••"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            {isSignup && (
              <p className="text-xs text-zinc-500">
                First-time setup: register as Admin. After logging in, you can create organizations
                and invite Instructors and Trainees.
              </p>
            )}
            {error && <p className="text-sm text-red-600">{error}</p>}
            <Button type="submit" className="w-full" disabled={submitting}>
              {submitting
                ? isSignup
                  ? "Creating account..."
                  : "Signing in..."
                : isSignup
                  ? "Sign up"
                  : "Sign in"}
            </Button>
          </form>
          <div className="mt-4 text-center text-sm text-zinc-500">
            {isSignup ? (
              <button type="button" onClick={toggleMode} className="underline hover:text-zinc-800">
                Already have an account? Sign in
              </button>
            ) : (
              <button type="button" onClick={toggleMode} className="underline hover:text-zinc-800">
                Don&apos;t have an account? Sign up
              </button>
            )}
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
