"use client"

import { useEffect, useState } from "react"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Building2, Users } from "lucide-react"

interface OrgSummary {
  id: string
  name: string
  createdAt: string
  userCount: number
}

export default function AdminDashboard() {
  const [orgs, setOrgs] = useState<OrgSummary[]>([])
  const [totalUsers, setTotalUsers] = useState(0)

  useEffect(() => {
    api.get<OrgSummary[]>("/api/admin/organizations").then((data) => {
      setOrgs(data)
      setTotalUsers(data.reduce((sum, o) => sum + o.userCount, 0))
    })
  }, [])

  return (
    <div>
      <Navbar title="Admin Dashboard" />
      <div className="p-6 space-y-6">
        <div className="grid gap-4 md:grid-cols-2">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Organizations</CardTitle>
              <Building2 className="h-4 w-4 text-zinc-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{orgs.length}</div>
            </CardContent>
          </Card>
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Users</CardTitle>
              <Users className="h-4 w-4 text-zinc-500" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{totalUsers}</div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
