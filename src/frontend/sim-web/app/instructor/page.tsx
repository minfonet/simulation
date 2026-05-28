"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { useRouter } from "next/navigation"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Gamepad2, Users, ClipboardCheck, AlertCircle, Plus } from "lucide-react"

interface SessionSummary {
  id: string
  status: string
  scenario: string
  traineeName?: string
  score?: number | null
}

export default function InstructorDashboard() {
  const router = useRouter()
  const [sessions, setSessions] = useState<SessionSummary[]>([])

  useEffect(() => {
    api.get<SessionSummary[]>("/api/instructor/sessions").then(setSessions)
  }, [])

  const activeCount = sessions.filter((s) => s.status === "Active").length
  const completedCount = sessions.filter((s) => s.status === "Completed").length

  const statusVariant = (status: string) => {
    switch (status) {
      case "Pending":
        return "warning"
      case "Active":
        return "default"
      case "Completed":
        return "success"
      default:
        return "secondary"
    }
  }

  const scoreVariant = (score: number) => {
    if (score >= 70) return "success"
    if (score >= 40) return "warning"
    return "destructive"
  }

  return (
    <div>
      <Navbar title="Instructor Dashboard" />
      <div className="p-6 space-y-6">
        {/* Metric Cards */}
        <div className="grid gap-4 md:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Sessions</CardTitle>
              <Gamepad2 className="h-4 w-4 text-zinc-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{sessions.length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Active</CardTitle>
              <Users className="h-4 w-4 text-zinc-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{activeCount}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Completed</CardTitle>
              <ClipboardCheck className="h-4 w-4 text-zinc-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{completedCount}</div>
            </CardContent>
          </Card>
          <Link href="/instructor/sessions" className="block">
            <Card className="cursor-pointer hover:border-zinc-300 transition-colors">
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">Pending Evaluations</CardTitle>
                <AlertCircle className="h-4 w-4 text-zinc-500" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{completedCount}</div>
                <p className="text-xs text-zinc-500 mt-1">Sessions awaiting evaluation</p>
              </CardContent>
            </Card>
          </Link>
        </div>

        {/* Recent Sessions Table */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>Recent Sessions</CardTitle>
            <Link href="/instructor/sessions">
              <Button size="sm" className="gap-1.5">
                <Plus className="h-4 w-4" />
                Create New Session
              </Button>
            </Link>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-zinc-200 text-left">
                    <th className="pb-3 font-medium text-zinc-500">Trainee</th>
                    <th className="pb-3 font-medium text-zinc-500">Scenario</th>
                    <th className="pb-3 font-medium text-zinc-500">Status</th>
                    <th className="pb-3 font-medium text-zinc-500">Score</th>
                  </tr>
                </thead>
                <tbody>
                  {sessions.length === 0 ? (
                    <tr>
                      <td colSpan={4} className="py-8 text-center text-sm text-zinc-500">
                        No sessions yet.
                      </td>
                    </tr>
                  ) : (
                    sessions.map((session) => (
                      <tr
                        key={session.id}
                        className="border-b border-zinc-100 hover:bg-zinc-50 cursor-pointer transition-colors"
                        onClick={() => router.push(`/instructor/sessions/${session.id}`)}
                      >
                        <td className="py-3 font-medium text-zinc-900">
                          {session.traineeName || "\u2014"}
                        </td>
                        <td className="py-3 text-zinc-600">{session.scenario}</td>
                        <td className="py-3">
                          <Badge variant={statusVariant(session.status)}>
                            {session.status}
                          </Badge>
                        </td>
                        <td className="py-3">
                          {session.score != null ? (
                            <Badge variant={scoreVariant(session.score)}>
                              {session.score}
                            </Badge>
                          ) : (
                            <span className="text-zinc-400">{"\u2014"}</span>
                          )}
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
