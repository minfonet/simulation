"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"

interface Organization {
  id: string
  name: string
  createdAt: string
  userCount: number
}

export default function OrganizationsPage() {
  const [orgs, setOrgs] = useState<Organization[]>([])
  const [name, setName] = useState("")

  useEffect(() => {
    api.get<Organization[]>("/api/admin/organizations").then(setOrgs)
  }, [])

  async function createOrg() {
    if (!name.trim()) return
    const org = await api.post<Organization>("/api/admin/organizations", { name })
    setOrgs((prev) => [...prev, org])
    setName("")
  }

  async function deleteOrg(id: string) {
    await api.delete(`/api/admin/organizations/${id}`)
    setOrgs((prev) => prev.filter((o) => o.id !== id))
  }

  return (
    <div>
      <Navbar title="Organizations" />
      <div className="p-6 space-y-6">
        <div className="flex gap-3">
          <Input
            placeholder="Organization name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            className="max-w-xs"
          />
          <Button onClick={createOrg}>Create</Button>
        </div>

        <div className="space-y-3">
          {orgs.map((org) => (
            <Card key={org.id}>
              <CardContent className="flex items-center justify-between py-4">
                <div>
                  <p className="font-medium">{org.name}</p>
                  <p className="text-sm text-zinc-500">
                    {org.userCount} user{org.userCount !== 1 ? "s" : ""}
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <Badge>{org.userCount} users</Badge>
                  <Button variant="destructive" size="sm" onClick={() => deleteOrg(org.id)}>
                    Delete
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      </div>
    </div>
  )
}
