import fs from 'fs';
import path from 'path';
import { randomUUID } from 'crypto';
import { CallStatus, Language, Lead, LeadReport } from './types';

/**
 * Mock database: een simpele JSON-file store met in-memory cache.
 * Bij de eerste start wordt data/seed-leads.json ingeladen als voorbeelddata.
 *
 * TODO (productie): vervangen door de eigen database/CRM van Borst Automotive.
 * De rest van de app praat alleen via de functies hieronder met de store,
 * zodat die vervanging op één plek kan gebeuren.
 */

const DATA_DIR = path.join(process.cwd(), 'data');
const DB_FILE = path.join(DATA_DIR, 'db.json');
const SEED_FILE = path.join(DATA_DIR, 'seed-leads.json');

let leads: Lead[] = [];

export function initDb(): void {
  fs.mkdirSync(DATA_DIR, { recursive: true });
  if (fs.existsSync(DB_FILE)) {
    leads = JSON.parse(fs.readFileSync(DB_FILE, 'utf-8'));
  } else if (fs.existsSync(SEED_FILE)) {
    leads = JSON.parse(fs.readFileSync(SEED_FILE, 'utf-8'));
    persist();
  }
}

function persist(): void {
  fs.writeFileSync(DB_FILE, JSON.stringify(leads, null, 2));
}

export function listLeads(): Lead[] {
  return [...leads].sort((a, b) => b.createdAt.localeCompare(a.createdAt));
}

export function getLead(id: string): Lead | undefined {
  return leads.find((l) => l.id === id);
}

export function findLeadByCallId(callId: string): Lead | undefined {
  return leads.find((l) => l.callId === callId);
}

export function createLead(input: {
  phoneNumber: string;
  model: string;
  language?: Language;
}): Lead {
  const now = new Date().toISOString();
  const lead: Lead = {
    id: randomUUID(),
    phoneNumber: input.phoneNumber.trim(),
    model: input.model.trim(),
    language: input.language ?? 'auto',
    status: 'not_called',
    callProvider: null,
    callId: null,
    createdAt: now,
    updatedAt: now,
    statusHistory: [{ status: 'not_called', at: now }],
    report: null,
  };
  leads.push(lead);
  persist();
  return lead;
}

export function updateLeadStatus(
  id: string,
  status: CallStatus,
  detail?: string
): Lead | undefined {
  const lead = getLead(id);
  if (!lead) return undefined;
  const now = new Date().toISOString();
  lead.status = status;
  lead.updatedAt = now;
  lead.statusHistory.push({ status, at: now, ...(detail ? { detail } : {}) });
  persist();
  return lead;
}

export function attachCall(
  id: string,
  provider: 'vapi' | 'mock',
  callId: string
): Lead | undefined {
  const lead = getLead(id);
  if (!lead) return undefined;
  lead.callProvider = provider;
  lead.callId = callId;
  lead.updatedAt = new Date().toISOString();
  persist();
  return lead;
}

export function saveReport(id: string, report: Partial<LeadReport>): Lead | undefined {
  const lead = getLead(id);
  if (!lead) return undefined;
  lead.report = { ...emptyReport(), ...lead.report, ...report };
  lead.updatedAt = new Date().toISOString();
  persist();
  return lead;
}

export function emptyReport(): LeadReport {
  return {
    language: null,
    appointmentRequested: null,
    preferredMoments: [],
    appointmentType: null,
    summary: null,
    usage: null,
    desiredOptions: [],
    fuelPreference: null,
    buyOrLease: null,
    businessOrPrivate: null,
    tradeIn: null,
    tradeInLicensePlate: null,
    tradeInMileage: null,
    objections: [],
    leadScore: null,
    recommendedNextStep: null,
    transcript: null,
  };
}
