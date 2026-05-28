"use client"

import { useEffect, useState } from "react"
import Link from "next/link"
import { api } from "@/lib/api"
import { Navbar } from "@/components/layout/navbar"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Building2, Users, Calendar, ArrowRight } from "lucide-react"

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
        {/* Metric Cards */}
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

        {/* Organizations Table */}
        <Card>
          <CardHeader>
            <CardTitle>Organizations</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-zinc-200 text-left">
                    <th className="pb-3 font-medium text-zinc-500">Name</th>
                    <th className="pb-3 font-medium text-zinc-500">Created At</th>
                    <th className="pb-3 font-medium text-zinc-500">Users</th>
                  </tr>
                </thead>
                <tbody>
                  {orgs.length === 0 ? (
                    <tr>
                      <td colSpan={3} className="py-8 text-center text-sm text-zinc-500">
                        No organizations found.
                      </td>
                    </tr>
                  ) : (
                    orgs.map((org) => (
                      <tr
                        key={org.id}
                        className="border-b border-zinc-100 hover:bg-zinc-50 cursor-pointer transition-colors"
                      >
                        <td className="py-3 font-medium text-zinc-900">{org.name}</td>
                        <td className="py-3 text-zinc-600">
                          <span className="inline-flex items-center gap-1.5">
                            <Calendar className="h-3.5 w-3.5 text-zinc-400" />
                            {new Date(org.createdAt).toLocaleDateString()}
                          </span>
                        </td>
                        <td className="py-3">
                          <span className="inline-flex items-center gap-1.5">
                            <Users className="h-3.5 w-3.5 text-zinc-400" />
                            {org.userCount}
                          </span>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <div className="flex flex-wrap gap-3">
          <Link href="/admin/organizations">
            <Button className="gap-2">
              <Building2 className="h-4 w-4" />
              Manage Organizations
              <ArrowRight className="h-4 w-4" />
            </Button>
          </Link>
          <Link href="/admin/users">
            <Button variant="outline" className="gap-2">
              <Users className="h-4 w-4" />
              Manage Users
              <ArrowRight className="h-4 w-4" />
            </Button>
          </Link>
        </div>
      </div>
    </div>
  )
}
