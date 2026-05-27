"use client"

import { useEffect, useState } from "react"
import { use } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"

interface SessionDetail {
  id: string
  instructorName: string
  scenario: string
  status: string
  score: number | null
  instructorNotes: string | null
  createdAt: string
  completedAt: string | null
}

interface TelemetryPoint {
  id: string
  timestamp: string
  speed: number
  steeringAngle: number
  positionX: number
  positionY: number
  positionZ: number
  collision: boolean
}

export default function TraineeSessionDetail({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const [session, setSession] = useState<SessionDetail | null>(null)
  const [telemetry, setTelemetry] = useState<TelemetryPoint[]>([])
  const [starting, setStarting] = useState(false)

  useEffect(() => {
    api.get<SessionDetail>(`/api/trainee/sessions/${id}`).then(setSession)
    api.get<TelemetryPoint[]>(`/api/telemetry/session/${id}`).then(setTelemetry)
  }, [id])

  async function startSession() {
    setStarting(true)
    await api.post(`/api/trainee/sessions/${id}/start`, {})
    const updated = await api.get<SessionDetail>(`/api/trainee/sessions/${id}`)
    setSession(updated)
    setStarting(false)
  }

  async function finishSession() {
    await api.post(`/api/trainee/sessions/${id}/finish`, {})
    const updated = await api.get<SessionDetail>(`/api/trainee/sessions/${id}`)
    setSession(updated)
  }

  if (!session) return null

  return (
    <div>
      <Navbar title={`Session Detail`} />
      <div className="p-6 space-y-6">
        <div className="flex items-center gap-4">
          <Badge
            variant={
              session.status === "Completed"
                ? "success"
                : session.status === "Active"
                  ? "warning"
                  : "secondary"
            }
          >
            {session.status}
          </Badge>
          <span className="text-sm text-zinc-500">
            Instructor: {session.instructorName}
          </span>
          <span className="text-sm text-zinc-500">Scenario: {session.scenario}</span>
          {session.score != null && (
            <span className="text-sm font-medium">Score: {session.score}%</span>
          )}
        </div>

        <div className="flex gap-3">
          {session.status === "Pending" && (
            <Button onClick={startSession} disabled={starting}>
              {starting ? "Starting..." : "Start Simulation"}
            </Button>
          )}
          {session.status === "Active" && (
            <Button onClick={finishSession} variant="destructive">
              Finish Simulation
            </Button>
          )}
        </div>

        {session.instructorNotes && (
          <Card>
            <CardHeader>
              <CardTitle>Instructor Notes</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-zinc-600">{session.instructorNotes}</p>
            </CardContent>
          </Card>
        )}

        <Card>
          <CardHeader>
            <CardTitle>Telemetry ({telemetry.length} points)</CardTitle>
          </CardHeader>
          <CardContent className="max-h-64 overflow-y-auto">
            {telemetry.length === 0 ? (
              <p className="text-sm text-zinc-500">No telemetry data available.</p>
            ) : (
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b text-left text-zinc-500">
                    <th className="pb-2 pr-2">Time</th>
                    <th className="pb-2 pr-2">Speed</th>
                    <th className="pb-2 pr-2">Steering</th>
                    <th className="pb-2">Collision</th>
                  </tr>
                </thead>
                <tbody>
                  {telemetry.slice(-50).map((t) => (
                    <tr key={t.id} className="border-b last:border-0">
                      <td className="py-1 pr-2">{new Date(t.timestamp).toLocaleTimeString()}</td>
                      <td className="py-1 pr-2">{t.speed.toFixed(1)}</td>
                      <td className="py-1 pr-2">{t.steeringAngle.toFixed(2)}</td>
                      <td className="py-1">{t.collision ? "⚠️" : "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
