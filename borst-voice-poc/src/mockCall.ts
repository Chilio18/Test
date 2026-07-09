import { randomUUID } from 'crypto';
import { Lead, LeadReport, Language } from './types';
import { attachCall, emptyReport, saveReport, updateLeadStatus } from './db';

/**
 * Mock mode: simuleert de volledele levenscyclus van een call zonder Vapi.
 * queued → in_progress → completed (met een plausibel leadrapport).
 * Zo is de hele UI- en dataflow te demonstreren zonder API keys of tunnel.
 */

export function startMockCall(lead: Lead): { callId: string } {
  const callId = `mock_${randomUUID()}`;
  attachCall(lead.id, 'mock', callId);
  updateLeadStatus(lead.id, 'queued', 'Mock call aangevraagd');

  setTimeout(() => {
    updateLeadStatus(lead.id, 'in_progress', 'Mock gesprek gestart');
  }, 2000);

  setTimeout(() => {
    saveReport(lead.id, buildMockReport(lead));
    updateLeadStatus(lead.id, 'completed', 'Mock gesprek afgerond, leadrapport opgeslagen');
  }, 10000);

  return { callId };
}

function resolveLanguage(language: Language): Exclude<Language, 'auto'> {
  return language === 'auto' ? 'nl' : language;
}

function buildMockReport(lead: Lead): LeadReport {
  const lang = resolveLanguage(lead.language);
  return {
    ...emptyReport(),
    language: lang,
    appointmentRequested: true,
    preferredMoments: ['zaterdag rond 11:00'],
    appointmentType: 'combinatie',
    summary:
      `Prospect bevestigde interesse in de ${lead.model}. Auto wordt vooral privé en voor ` +
      'woon-werkverkeer gebruikt; trekhaak gewenst voor een fietsendrager. Voorkeur voor ' +
      'hybride met automaat. Er is een inruilauto. Afspraak gewenst voor proefrit plus ' +
      'offertegesprek en inruiltaxatie op zaterdagochtend.',
    usage: 'Privé, woon-werkverkeer, fietsendrager op trekhaak',
    desiredOptions: ['automaat', 'trekhaak', 'navigatie', 'stoelverwarming'],
    fuelPreference: 'hybride',
    buyOrLease: 'kopen',
    businessOrPrivate: 'particulier',
    tradeIn: true,
    tradeInLicensePlate: 'AB-123-C',
    tradeInMileage: 'ca. 95.000 km',
    objections: ['wil eerst duidelijkheid over inruilwaarde'],
    leadScore: 5,
    recommendedNextStep:
      'Afspraak bevestigen voor zaterdag 11:00 (proefrit + offerte + taxatie) en een ' +
      `${lead.model} met trekhaak en automaat klaarzetten.`,
    transcript: mockTranscript(lead, lang),
  };
}

function mockTranscript(lead: Lead, lang: Exclude<Language, 'auto'>): string {
  if (lang === 'en') {
    return [
      `Agent: Good afternoon, you are speaking with the phone assistant of Borst Automotive. You recently showed interest in the ${lead.model}. Is this a good time?`,
      'Prospect: Yes, sure.',
      'Agent: Great. Are you looking at a new or a used car?',
      'Prospect: Probably new, with an automatic gearbox and a tow bar.',
      'Agent: Noted. Would Saturday around eleven work for a test drive and a quotation?',
      'Prospect: Saturday works.',
      'Agent: Perfect, a colleague will confirm the appointment. Thank you!',
    ].join('\n');
  }
  if (lang === 'da') {
    return [
      `Agent: Goddag, du taler med telefonassistenten hos Borst Automotive. Du har vist interesse for ${lead.model}. Passer det nu?`,
      'Prospect: Ja, det er fint.',
      'Agent: Dejligt. Kigger du på en ny eller brugt bil?',
      'Prospect: Nok en ny, med automatgear og anhængertræk.',
      'Agent: Noteret. Kunne lørdag omkring klokken elleve passe til en prøvetur og et tilbud?',
      'Prospect: Lørdag passer fint.',
      'Agent: Perfekt, en kollega bekræfter aftalen. Tak!',
    ].join('\n');
  }
  return [
    `Agent: Goedemiddag, u spreekt met de telefonische assistent van Borst Automotive. U heeft interesse getoond in de ${lead.model}. Komt het gelegen?`,
    'Prospect: Ja hoor, dat komt goed uit.',
    'Agent: Fijn. Kijkt u naar een nieuwe of een gebruikte auto?',
    'Prospect: Waarschijnlijk nieuw, met automaat en een trekhaak voor de fietsendrager.',
    'Agent: Genoteerd. Heeft u een auto om in te ruilen?',
    'Prospect: Ja, kenteken AB-123-C, ongeveer vijfennegentigduizend kilometer.',
    'Agent: Dank u. Zou zaterdag rond elf uur passen voor een proefrit met offertegesprek en inruiltaxatie?',
    'Prospect: Zaterdag past prima.',
    'Agent: Perfect, een medewerker bevestigt de afspraak. Fijne dag!',
  ].join('\n');
}
