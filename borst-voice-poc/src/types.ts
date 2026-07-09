export type Language = 'auto' | 'nl' | 'en' | 'da';

export type CallStatus =
  | 'not_called' // nog niet gebeld
  | 'queued' // call aangevraagd, wacht op verbinding
  | 'in_progress' // bezig
  | 'completed' // afgerond
  | 'failed'; // mislukt

export type AppointmentType = 'proefrit' | 'offerte' | 'taxatie' | 'combinatie';

export interface LeadReport {
  language: Exclude<Language, 'auto'> | null;
  appointmentRequested: boolean | null;
  preferredMoments: string[];
  appointmentType: AppointmentType | null;
  summary: string | null;
  usage: string | null;
  desiredOptions: string[];
  fuelPreference: string | null;
  buyOrLease: 'kopen' | 'leasen' | null;
  businessOrPrivate: 'zakelijk' | 'particulier' | null;
  tradeIn: boolean | null;
  tradeInLicensePlate: string | null;
  tradeInMileage: string | null;
  objections: string[];
  leadScore: 1 | 2 | 3 | 4 | 5 | null;
  recommendedNextStep: string | null;
  transcript: string | null;
}

export interface StatusEvent {
  status: CallStatus;
  at: string;
  detail?: string;
}

export interface Lead {
  id: string;
  phoneNumber: string;
  model: string;
  language: Language;
  status: CallStatus;
  callProvider: 'vapi' | 'mock' | null;
  callId: string | null;
  createdAt: string;
  updatedAt: string;
  statusHistory: StatusEvent[];
  report: LeadReport | null;
}
