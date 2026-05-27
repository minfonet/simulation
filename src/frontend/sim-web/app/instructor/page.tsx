"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Gamepad2, Users, ClipboardCheck } from "lucide-react"

interface SessionSummary {
  id: string
  status: string
}

interface TraineeInfo {
  id: string
  name: string
}

export default function InstructorDashboard() {
  const [sessions, setSessions] = useState<SessionSummary[]>([])
  const [trainees, setTrainees] = useState<TraineeInfo[]>([])

  useEffect(() => {
    api.get<SessionSummary[]>("/api/instructor/sessions").then(setSessions)
    api.get<TraineeInfo[]>("/api/instructor/trainees").then(setTrainees)
  }, [])

  const activeCount = sessions.filter((s) => s.status === "Active").length
  const completedCount = sessions.filter((s) => s.status === "Completed").length

  return (
    <div>
      <Navbar title="Instructor Dashboard" />
      <div className="p-6">
        <div className="grid gap-4 md:grid-cols-3">
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
        </div>

        <div className="mt-6">
          <Card>
            <CardHeader>
              <CardTitle>Assigned Trainees</CardTitle>
            </CardHeader>
            <CardContent>
              {trainees.length === 0 ? (
                <p className="text-sm text-zinc-500">No trainees assigned yet.</p>
              ) : (
                <ul className="space-y-2">
                  {trainees.map((t) => (
                    <li key={t.id} className="text-sm">
                      {t.name}
                    </li>
                  ))}
                </ul>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
