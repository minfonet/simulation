"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Gamepad2, ClipboardCheck } from "lucide-react"

interface SessionSummary {
  id: string
  status: string
}

interface EvaluationSummary {
  id: string
  score: number
}

export default function TraineeDashboard() {
  const [sessions, setSessions] = useState<SessionSummary[]>([])
  const [evaluations, setEvaluations] = useState<EvaluationSummary[]>([])

  useEffect(() => {
    api.get<SessionSummary[]>("/api/trainee/sessions").then(setSessions)
    api.get<EvaluationSummary[]>("/api/trainee/evaluations").then(setEvaluations)
  }, [])

  const pending = sessions.filter((s) => s.status === "Pending").length

  return (
    <div>
      <Navbar title="Trainee Dashboard" />
      <div className="p-6">
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
              <Gamepad2 className="h-4 w-4 text-zinc-500" />
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
      </div>
    </div>
  )
}
