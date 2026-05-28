"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Card, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import Link from "next/link"
import { useRouter } from "next/navigation"

interface Session {
  id: string
  instructorName: string
  scenario: string
  status: string
  score: number | null
  instructorNotes: string | null
  createdAt: string
}

const statusColor: Record<string, "default" | "success" | "warning" | "secondary" | "destructive"> = {
  Pending: "secondary",
  Active: "success",
  Completed: "default",
  Failed: "destructive",
}

export default function TraineeSessions() {
  const router = useRouter()
  const [sessions, setSessions] = useState<Session[]>([])

  useEffect(() => {
    api.get<Session[]>("/api/trainee/sessions").then(setSessions)
  }, [])

  function startSession(id: string) {
    router.push(`/trainee/sessions/${id}`)
  }

  return (
    <div>
      <Navbar title="My Sessions" />
      <div className="p-6 space-y-3">
        {sessions.length === 0 ? (
          <p className="text-sm text-zinc-500">No sessions assigned yet.</p>
        ) : (
          sessions.map((s) => (
            <Card key={s.id}>
              <CardContent className="flex items-center justify-between py-4">
                <div>
                  <p className="font-medium">
                    {s.scenario} — {s.instructorName}
                  </p>
                  <p className="text-xs text-zinc-400">
                    Created {new Date(s.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <Badge variant={statusColor[s.status] ?? "default"}>{s.status}</Badge>
                  {s.status === "Pending" && (
                    <Button size="sm" onClick={() => startSession(s.id)}>
                      Start
                    </Button>
                  )}
                  {(s.status === "Completed" || s.status === "Failed") && (
                    <Link href={`/trainee/sessions/${s.id}`}>
                      <Button size="sm" variant="outline">
                        View
                      </Button>
                    </Link>
                  )}
                </div>
              </CardContent>
            </Card>
          ))
        )}
      </div>
    </div>
  )
}
