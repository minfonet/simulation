"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { useAuth } from "@/lib/auth-context"
import { Navbar } from "@/components/layout/navbar"
import { Button } from "@/components/ui/button"
import { Select } from "@/components/ui/select"
import { Card, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import Link from "next/link"

interface TraineeOption {
  id: string
  name: string
}

interface ScenarioPreset {
  id: string
  name: string
  description: string
  godotScenePath: string
}

interface Session {
  id: string
  traineeId: string
  traineeName: string
  scenario: string
  status: string
  score: number | null
  createdAt: string
}

const statusColor: Record<string, "default" | "success" | "warning" | "secondary" | "destructive"> = {
  Pending: "secondary",
  Active: "success",
  Completed: "default",
  Failed: "destructive",
}

export default function InstructorSessions() {
  const { user } = useAuth()
  const [sessions, setSessions] = useState<Session[]>([])
  const [trainees, setTrainees] = useState<TraineeOption[]>([])
  const [selectedTrainee, setSelectedTrainee] = useState("")
  const [scenario, setScenario] = useState("default")
  const [presets, setPresets] = useState<ScenarioPreset[]>([])
  const [presetsLoading, setPresetsLoading] = useState(true)

  useEffect(() => {
    api.get<Session[]>("/api/instructor/sessions").then(setSessions)
    api.get<TraineeOption[]>("/api/instructor/trainees").then(setTrainees)
    api.get<ScenarioPreset[]>("/api/instructor/scenario-presets").then((data) => {
      setPresets(data)
      const defaultPreset = data.find((p) => p.id === "default")
      if (defaultPreset) setScenario(defaultPreset.id)
      setPresetsLoading(false)
    }).catch(() => setPresetsLoading(false))
  }, [])

  async function createSession() {
    if (!selectedTrainee) return
    const session = await api.post<Session>("/api/instructor/sessions", {
      traineeId: selectedTrainee,
      scenario,
    })
    setSessions((prev) => [session, ...prev])
  }

  return (
    <div>
      <Navbar title="Sessions" />
      <div className="p-6 space-y-6">
        <Card>
          <CardContent className="pt-6 space-y-3">
            <div className="flex gap-3">
              <Select value={selectedTrainee} onChange={(e) => setSelectedTrainee(e.target.value)}>
                <option value="">Select trainee...</option>
                {trainees.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name}
                  </option>
                ))}
              </Select>
              <div className="flex flex-col gap-1 max-w-[200px]">
                <Select value={scenario} onChange={(e) => setScenario(e.target.value)}>
                  {presetsLoading ? (
                    <option value="">Loading presets...</option>
                  ) : presets.length === 0 ? (
                    <option value="">No presets available</option>
                  ) : (
                    presets.map((p) => (
                      <option key={p.id} value={p.id}>
                        {p.name}
                      </option>
                    ))
                  )}
                </Select>
                {(() => {
                  const selectedPreset = presets.find((p) => p.id === scenario)
                  return selectedPreset?.description ? (
                    <p className="text-xs text-zinc-500">{selectedPreset.description}</p>
                  ) : null
                })()}
              </div>
              <Button onClick={createSession}>Create Session</Button>
            </div>
          </CardContent>
        </Card>

        <div className="space-y-3">
          {sessions.map((s) => (
            <Link key={s.id} href={`/instructor/sessions/${s.id}`}>
              <Card className="cursor-pointer hover:bg-zinc-50 transition-colors">
                <CardContent className="flex items-center justify-between py-4">
                  <div>
                    <p className="font-medium">{s.traineeName}</p>
                    <p className="text-sm text-zinc-500">{s.scenario}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <Badge variant={statusColor[s.status] ?? "default"}>{s.status}</Badge>
                    {s.score != null && (
                      <span className="text-sm font-medium">{s.score}%</span>
                    )}
                  </div>
                </CardContent>
              </Card>
            </Link>
          ))}
        </div>
      </div>
    </div>
  )
}
