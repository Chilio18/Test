import { config } from './config';
import { Lead, LeadReport } from './types';
import { buildAgentPrompt, firstMessage, saveLeadReportTool } from './prompts/automotive-agent';
import { emptyReport } from './db';

/**
 * Dunne client voor de Vapi API. We gebruiken een "transient assistant":
 * de volledige assistant-configuratie (prompt, model, tools) wordt per call
 * meegestuurd, zodat de prompt en het model altijd actueel zijn en er niets
 * in het Vapi-dashboard beheerd hoeft te worden.
 *
 * TODO (productie):
 *  - retries + nette foutafhandeling rond de Vapi API;
 *  - assistant eventueel als vaste Vapi-assistant beheren i.p.v. transient;
 *  - opt-out/bel-me-niet-register checken vóór het uitbellen.
 */

export function buildAssistantConfig(lead: Lead) {
  const systemPrompt = buildAgentPrompt({
    model: lead.model,
    language: lead.language,
    phoneNumber: lead.phoneNumber,
  });

  return {
    name: 'Borst Automotive Leadassistent',
    firstMessage: firstMessage(lead.model, lead.language),
    // Realtime (speech-to-speech) model; naam configureerbaar via env.
    model: {
      provider: 'openai',
      model: config.openai.realtimeModel, // default: gpt-realtime-2
      messages: [{ role: 'system', content: systemPrompt }],
      tools: [saveLeadReportTool, { type: 'endCall' }],
    },
    voice: {
      provider: 'openai',
      voiceId: config.openai.realtimeVoice,
    },
    // Vapi stuurt server messages (status, tool-calls, end-of-call-report)
    // naar onze webhook.
    server: {
      url: `${config.publicBaseUrl}/api/webhooks/vapi`,
      secret: config.vapi.webhookSecret || undefined,
    },
    serverMessages: ['status-update', 'tool-calls', 'end-of-call-report'],
    // Gespreksanalyse van Vapi als vangnet naast onze eigen tool call.
    analysisPlan: {
      summaryPlan: { enabled: true },
    },
    maxDurationSeconds: 600,
  };
}

export async function startVapiCall(lead: Lead): Promise<{ callId: string }> {
  const payload = {
    phoneNumberId: config.vapi.phoneNumberId,
    customer: { number: lead.phoneNumber },
    assistant: buildAssistantConfig(lead),
    metadata: { leadId: lead.id },
  };

  const res = await fetch(`${config.vapi.baseUrl}/call`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${config.vapi.apiKey}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const body = await res.text();
    throw new Error(`Vapi call request failed (${res.status}): ${body}`);
  }

  const data = (await res.json()) as { id: string };
  return { callId: data.id };
}

/**
 * Zet de snake_case argumenten van de save_lead_report tool call om naar
 * het LeadReport-formaat van de mock database.
 */
export function toolArgsToReport(args: Record<string, unknown>): Partial<LeadReport> {
  const base = emptyReport();
  const s = (v: unknown): string | null => (typeof v === 'string' && v.trim() ? v.trim() : null);
  const list = (v: unknown): string[] =>
    Array.isArray(v) ? v.filter((x): x is string => typeof x === 'string') : [];
  const bool = (v: unknown): boolean | null => (typeof v === 'boolean' ? v : null);

  const score = typeof args.lead_score === 'number' ? Math.round(args.lead_score) : null;

  return {
    ...base,
    language: (['nl', 'en', 'da'].includes(args.language as string)
      ? args.language
      : null) as LeadReport['language'],
    appointmentRequested: bool(args.appointment_requested),
    preferredMoments: list(args.preferred_moments),
    appointmentType: (['proefrit', 'offerte', 'taxatie', 'combinatie'].includes(
      args.appointment_type as string
    )
      ? args.appointment_type
      : null) as LeadReport['appointmentType'],
    summary: s(args.summary),
    usage: s(args.usage),
    desiredOptions: list(args.desired_options),
    fuelPreference: s(args.fuel_preference),
    buyOrLease: (['kopen', 'leasen'].includes(args.buy_or_lease as string)
      ? args.buy_or_lease
      : null) as LeadReport['buyOrLease'],
    businessOrPrivate: (['zakelijk', 'particulier'].includes(args.business_or_private as string)
      ? args.business_or_private
      : null) as LeadReport['businessOrPrivate'],
    tradeIn: bool(args.trade_in),
    tradeInLicensePlate: s(args.trade_in_license_plate),
    tradeInMileage: s(args.trade_in_mileage),
    objections: list(args.objections),
    leadScore: (score && score >= 1 && score <= 5 ? score : null) as LeadReport['leadScore'],
    recommendedNextStep: s(args.recommended_next_step),
    transcript: s(args.notes),
  };
}
