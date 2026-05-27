"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Card, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"

interface Evaluation {
  id: string
  sessionId: string
  score: number
  comments: string | null
  createdAt: string
}

export default function InstructorEvaluations() {
  const [evaluations, setEvaluations] = useState<Evaluation[]>([])

  useEffect(() => {
    api.get<Evaluation[]>("/api/instructor/evaluations").then(setEvaluations)
  }, [])

  return (
    <div>
      <Navbar title="Evaluations" />
      <div className="p-6 space-y-3">
        {evaluations.length === 0 ? (
          <p className="text-sm text-zinc-500">No evaluations yet.</p>
        ) : (
          evaluations.map((e) => (
            <Card key={e.id}>
              <CardContent className="flex items-center justify-between py-4">
                <div>
                  <p className="font-medium">Score: {e.score}%</p>
                  {e.comments && (
                    <p className="text-sm text-zinc-500">{e.comments}</p>
                  )}
                  <p className="text-xs text-zinc-400">
                    {new Date(e.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <Badge variant={e.score >= 70 ? "success" : e.score >= 40 ? "warning" : "destructive"}>
                  {e.score}%
                </Badge>
              </CardContent>
            </Card>
          ))
        )}
      </div>
    </div>
  )
}
