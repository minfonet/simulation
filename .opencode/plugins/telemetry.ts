import type { Plugin } from "@opencode-ai/plugin"
import { appendFileSync } from "fs"
import { join } from "path"

const FILE = ".opencode/telemetry.jsonl"
const startTimes = new Map<number, number>()
let callId = 0

export default (async ({ directory }) => {
  const filePath = join(directory, FILE)

  function log(type: string, data: Record<string, unknown>) {
    try {
      appendFileSync(filePath, JSON.stringify({ t: type, ts: Date.now(), data }) + "\n")
    } catch { /* telemetry must never crash the session */ }
  }

  log("session_start", { pid: process.pid })

  return {
    "chat.params": (params: any) => {
      try {
        const msgs = params.messages
        log("chat_request", {
          model: params.model,
          maxTokens: params.max_tokens,
          messageCount: msgs?.length,
          inputChars: msgs ? JSON.stringify(msgs).length : 0,
          temp: params.temperature,
        })
      } catch { /* ignore */ }
    },

    "tool.execute.before": (input: any, output: any) => {
      callId++
      startTimes.set(callId, Date.now())
      output.args = { ...output.args, __tel: callId }
    },

    "tool.execute.after": (input: any, output: any) => {
      try {
        const id = output.args?.__tel
        const started = id ? startTimes.get(id) : undefined
        if (id) startTimes.delete(id)
        log("tool_exec", {
          tool: input.name || input.tool || "?",
          duration: started ? Date.now() - started : 0,
          hasError: !!output.error,
          errorMsg: output.error?.message || null,
        })
      } catch { /* ignore */ }
    },
  }
}) satisfies Plugin
