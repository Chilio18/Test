import { Router } from 'express';
import { config } from '../config';
import { attachCall, createLead, getLead, updateLeadStatus } from '../db';
import { startMockCall } from '../mockCall';
import { startVapiCall } from '../vapi';
import { Language } from '../types';

export const callsRouter = Router();

/**
 * Call starten. Accepteert óf een bestaand leadId, óf direct
 * { phoneNumber, model, language } — dan wordt eerst een lead aangemaakt.
 */
callsRouter.post('/', async (req, res) => {
  const { leadId, phoneNumber, model, language } = req.body ?? {};

  let lead = leadId ? getLead(leadId) : undefined;
  if (leadId && !lead) {
    return res.status(404).json({ error: 'Lead niet gevonden' });
  }
  if (!lead) {
    if (!phoneNumber || !model) {
      return res.status(400).json({ error: 'phoneNumber en model zijn verplicht' });
    }
    lead = createLead({ phoneNumber, model, language: language as Language });
  }

  if (lead.status === 'queued' || lead.status === 'in_progress') {
    return res.status(409).json({ error: 'Er loopt al een call voor deze lead' });
  }

  try {
    if (config.mockMode) {
      const { callId } = startMockCall(lead);
      return res.status(202).json({ lead: getLead(lead.id), callId, provider: 'mock' });
    }

    const { callId } = await startVapiCall(lead);
    attachCall(lead.id, 'vapi', callId);
    updateLeadStatus(lead.id, 'queued', 'Vapi call aangevraagd');
    return res.status(202).json({ lead: getLead(lead.id), callId, provider: 'vapi' });
  } catch (err) {
    const message = err instanceof Error ? err.message : String(err);
    updateLeadStatus(lead.id, 'failed', `Call starten mislukt: ${message}`);
    return res.status(502).json({ error: 'Call starten mislukt', detail: message });
  }
});
