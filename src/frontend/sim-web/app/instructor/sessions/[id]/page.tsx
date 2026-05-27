"use client"

import { useEffect, useState } from "react"
import { use } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Textarea } from "@/components/ui/textarea"
import { Input } from "@/components/ui/input"

interface SessionDetail {
  id: string
  traineeName: string
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

export default function InstructorSessionDetail({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params)
  const [session, setSession] = useState<SessionDetail | null>(null)
  const [telemetry, setTelemetry] = useState<TelemetryPoint[]>([])
  const [score, setScore] = useState("")
  const [comments, setComments] = useState("")

  useEffect(() => {
    api.get<SessionDetail>(`/api/instructor/sessions/${id}`).then((s) => {
      setSession(s)
      if (s.score != null) setScore(String(s.score))
      if (s.instructorNotes) setComments(s.instructorNotes)
    })
    api.get<TelemetryPoint[]>(`/api/telemetry/session/${id}`).then(setTelemetry)
  }, [id])

  async function evaluate() {
    await api.post(`/api/instructor/sessions/${id}/evaluate`, {
      score: Number(score),
      comments,
    })
    const updated = await api.get<SessionDetail>(`/api/instructor/sessions/${id}`)
    setSession(updated)
  }

  if (!session) return null

  return (
    <div>
      <Navbar title={`Session: ${session.traineeName}`} />
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
          <span className="text-sm text-zinc-500">Scenario: {session.scenario}</span>
          {session.score != null && (
            <span className="text-sm font-medium">Score: {session.score}%</span>
          )}
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle>Telemetry ({telemetry.length} points)</CardTitle>
            </CardHeader>
            <CardContent className="max-h-96 overflow-y-auto">
              {telemetry.length === 0 ? (
                <p className="text-sm text-zinc-500">No telemetry data yet.</p>
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

          <Card>
            <CardHeader>
              <CardTitle>Evaluation</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">Score (0-100)</label>
                <Input
                  type="number"
                  min={0}
                  max={100}
                  value={score}
                  onChange={(e) => setScore(e.target.value)}
                  disabled={session.status !== "Completed"}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">Comments</label>
                <Textarea
                  value={comments}
                  onChange={(e) => setComments(e.target.value)}
                  disabled={session.status !== "Completed"}
                />
              </div>
              <Button
                onClick={evaluate}
                disabled={session.status !== "Completed"}
                className="w-full"
              >
                {session.score != null ? "Update Evaluation" : "Submit Evaluation"}
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
