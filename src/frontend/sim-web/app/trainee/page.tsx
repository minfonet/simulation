"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Gamepad2, ClipboardCheck, Clock, Play, Calendar } from "lucide-react"

interface SessionSummary {
  id: string
  status: string
  scenario: string
}

interface EvaluationSummary {
  id: string
  score: number
  instructorName?: string
  createdAt?: string
}

export default function TraineeDashboard() {
  const [sessions, setSessions] = useState<SessionSummary[]>([])
  const [evaluations, setEvaluations] = useState<EvaluationSummary[]>([])

  useEffect(() => {
    api.get<SessionSummary[]>("/api/trainee/sessions").then(setSessions)
    api.get<EvaluationSummary[]>("/api/trainee/evaluations").then(setEvaluations)
  }, [])

  const pending = sessions.filter((s) => s.status === "Pending").length
  const pendingSessions = sessions.filter((s) => s.status === "Pending")

  const scoreVariant = (score: number) => {
    if (score >= 70) return "success"
    if (score >= 40) return "warning"
    return "destructive"
  }

  return (
    <div>
      <Navbar title="Trainee Dashboard" />
      <div className="p-6 space-y-6">
        {/* Metric Cards */}
        <div className="grid gap-4 md:grid-cols-3">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">My Sessions</CardTitle>
              <Gamepad2 className="h-4 w-4 text-zinc-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{sessions.length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Pending</CardTitle>
              <Clock className="h-4 w-4 text-zinc-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{pending}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Evaluations</CardTitle>
              <ClipboardCheck className="h-4 w-4 text-zinc-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{evaluations.length}</div>
            </CardContent>
          </Card>
        </div>

        {/* Pending Sessions Table */}
        <Card>
          <CardHeader>
            <CardTitle>Pending Sessions</CardTitle>
          </CardHeader>
          <CardContent>
            {pendingSessions.length === 0 ? (
              <p className="text-sm text-zinc-500 py-2">No pending sessions. All caught up!</p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-zinc-200 text-left">
                      <th className="pb-3 font-medium text-zinc-500">Scenario</th>
                      <th className="pb-3 font-medium text-zinc-500">Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {pendingSessions.map((session) => (
                      <tr key={session.id} className="border-b border-zinc-100">
                        <td className="py-3 font-medium text-zinc-900">{session.scenario}</td>
                        <td className="py-3">
                          <Link href={`/trainee/sessions/${session.id}`}>
                            <Button size="sm" className="gap-1.5">
                              <Play className="h-3.5 w-3.5" />
                              Start
                            </Button>
                          </Link>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Recent Evaluations Table */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Evaluations</CardTitle>
          </CardHeader>
          <CardContent>
            {evaluations.length === 0 ? (
              <p className="text-sm text-zinc-500 py-2">
                No evaluations yet. Complete a session to receive feedback.
              </p>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-zinc-200 text-left">
                      <th className="pb-3 font-medium text-zinc-500">Instructor</th>
                      <th className="pb-3 font-medium text-zinc-500">Score</th>
                      <th className="pb-3 font-medium text-zinc-500">Date</th>
                    </tr>
                  </thead>
                  <tbody>
                    {evaluations.map((ev) => (
                      <tr key={ev.id} className="border-b border-zinc-100">
                        <td className="py-3 font-medium text-zinc-900">
                          {ev.instructorName || "\u2014"}
                        </td>
                        <td className="py-3">
                          <Badge variant={scoreVariant(ev.score)}>{ev.score}</Badge>
                        </td>
                        <td className="py-3 text-zinc-600">
                          <span className="inline-flex items-center gap-1.5">
                            <Calendar className="h-3.5 w-3.5 text-zinc-400" />
                            {ev.createdAt
                              ? new Date(ev.createdAt).toLocaleDateString()
                              : "\u2014"}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
