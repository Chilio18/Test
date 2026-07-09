import { Router } from 'express';
import { createLead, getLead, listLeads, saveReport } from '../db';
import { Language } from '../types';

export const leadsRouter = Router();

const LANGUAGES: Language[] = ['auto', 'nl', 'en', 'da'];

/** Leads ophalen (recentste eerst). */
leadsRouter.get('/', (_req, res) => {
  res.json(listLeads());
});

/** Eén lead met leadrapport ophalen. */
leadsRouter.get('/:id', (req, res) => {
  const lead = getLead(req.params.id);
  if (!lead) return res.status(404).json({ error: 'Lead niet gevonden' });
  res.json(lead);
});

/** Lead aanmaken (zonder direct te bellen). */
leadsRouter.post('/', (req, res) => {
  const { phoneNumber, model, language } = req.body ?? {};
  if (!phoneNumber || typeof phoneNumber !== 'string') {
    return res.status(400).json({ error: 'phoneNumber is verplicht' });
  }
  if (!model || typeof model !== 'string') {
    return res.status(400).json({ error: 'model is verplicht' });
  }
  if (language !== undefined && !LANGUAGES.includes(language)) {
    return res.status(400).json({ error: `language moet één van ${LANGUAGES.join(', ')} zijn` });
  }
  const lead = createLead({ phoneNumber, model, language });
  res.status(201).json(lead);
});

/**
 * Leadrapport (deels) opslaan — mock endpoint.
 * In de echte flow gebeurt dit via de Vapi webhook (tool call van de agent),
 * maar dit endpoint maakt het testen en latere integratie eenvoudig.
 */
leadsRouter.post('/:id/report', (req, res) => {
  const lead = saveReport(req.params.id, req.body ?? {});
  if (!lead) return res.status(404).json({ error: 'Lead niet gevonden' });
  res.json(lead);
});
