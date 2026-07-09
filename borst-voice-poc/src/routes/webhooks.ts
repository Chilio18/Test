import { Router } from 'express';
import { config } from '../config';
import { findLeadByCallId, getLead, saveReport, updateLeadStatus } from '../db';
import { toolArgsToReport } from '../vapi';
import { Lead } from '../types';

export const webhooksRouter = Router();

/**
 * Webhook voor Vapi "server messages".
 * Vapi POST hier o.a.: status-update, tool-calls en end-of-call-report.
 * Docs: https://docs.vapi.ai/server-url
 *
 * TODO (productie): payload-validatie met een schema (bijv. zod) en
 * signature-verificatie i.p.v. alleen een shared secret header.
 */
webhooksRouter.post('/vapi', (req, res) => {
  // Eenvoudige verificatie via het shared secret dat Vapi meestuurt.
  if (config.vapi.webhookSecret) {
    const secret = req.header('x-vapi-secret');
    if (secret !== config.vapi.webhookSecret) {
      return res.status(401).json({ error: 'Ongeldig webhook secret' });
    }
  }

  const message = req.body?.message;
  if (!message?.type) {
    return res.status(400).json({ error: 'Ongeldige webhook payload' });
  }

  const lead = resolveLead(message);
  if (!lead) {
    // Onbekende call: 200 teruggeven zodat Vapi niet blijft retryen.
    console.warn('[webhook] Geen lead gevonden voor webhook message', message.type);
    return res.json({ ok: true });
  }

  switch (message.type) {
    case 'status-update': {
      const status = String(message.status ?? '');
      if (status === 'ringing' || status === 'queued') {
        updateLeadStatus(lead.id, 'queued', `Vapi status: ${status}`);
      } else if (status === 'in-progress') {
        updateLeadStatus(lead.id, 'in_progress', 'Gesprek gestart');
      } else if (status === 'ended') {
        const reason = String(message.endedReason ?? '');
        const failed = /(failed|error|busy|no-answer|voicemail)/i.test(reason);
        // Alleen naar completed als er niet al een eindstatus staat.
        if (lead.status !== 'completed' && lead.status !== 'failed') {
          updateLeadStatus(
            lead.id,
            failed ? 'failed' : 'completed',
            `Gesprek beëindigd (${reason || 'onbekende reden'})`
          );
        }
      }
      return res.json({ ok: true });
    }

    case 'tool-calls': {
      // De voice agent roept save_lead_report aan; sla het rapport op en
      // geef per tool call een resultaat terug zodat het gesprek doorloopt.
      const toolCalls: any[] = message.toolCallList ?? message.toolCalls ?? [];
      const results = toolCalls.map((tc) => {
        const name = tc?.function?.name ?? tc?.name;
        if (name === 'save_lead_report') {
          const rawArgs = tc?.function?.arguments ?? tc?.arguments ?? {};
          const args = typeof rawArgs === 'string' ? safeParse(rawArgs) : rawArgs;
          saveReport(lead.id, toolArgsToReport(args));
          return { toolCallId: tc.id, result: 'Leadrapport opgeslagen.' };
        }
        return { toolCallId: tc?.id, result: `Onbekende tool: ${name}` };
      });
      return res.json({ results });
    }

    case 'end-of-call-report': {
      // Vangnet: als de agent de tool niet aanriep, bewaren we in elk geval
      // het transcript en de samenvatting van Vapi.
      const transcript = message.transcript ?? message.artifact?.transcript ?? null;
      const summary = message.summary ?? message.analysis?.summary ?? null;
      const current = getLead(lead.id);
      saveReport(lead.id, {
        ...(current?.report?.transcript ? {} : { transcript }),
        ...(current?.report?.summary ? {} : { summary }),
      });
      if (current && current.status !== 'failed') {
        updateLeadStatus(lead.id, 'completed', 'End-of-call-report ontvangen');
      }
      return res.json({ ok: true });
    }

    default:
      return res.json({ ok: true });
  }
});

function resolveLead(message: any): Lead | undefined {
  const leadId: string | undefined =
    message?.call?.metadata?.leadId ?? message?.metadata?.leadId;
  if (leadId) {
    const byId = getLead(leadId);
    if (byId) return byId;
  }
  const callId: string | undefined = message?.call?.id ?? message?.callId;
  return callId ? findLeadByCallId(callId) : undefined;
}

function safeParse(raw: string): Record<string, unknown> {
  try {
    return JSON.parse(raw);
  } catch {
    return {};
  }
}
