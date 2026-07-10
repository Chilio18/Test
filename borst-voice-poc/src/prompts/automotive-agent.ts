import fs from 'fs';
import path from 'path';
import { Language } from '../types';

/**
 * De volledige system prompt staat in agent-prompt.md (projectroot) en is
 * daar zonder codewijziging aan te passen. Dit bestand leest de prompt in
 * en vult de call-specifieke placeholders.
 *
 * TODO (productie): prompt uit een CMS of database laden zodat marketing/
 * sales de prompt kan beheren, met versienummer per gesprek gelogd.
 */

const PROMPT_FILE = path.join(process.cwd(), 'agent-prompt.md');

const LANGUAGE_LABELS: Record<Language, string> = {
  auto: 'auto (detecteer de taal van de prospect; open in het Nederlands)',
  nl: 'Nederlands',
  en: 'Engels',
  da: 'Deens',
};

export function buildAgentPrompt(params: {
  model: string;
  language: Language;
  phoneNumber: string;
}): string {
  // Het HTML-commentaarblok bovenin agent-prompt.md is documentatie voor
  // beheerders en hoort niet in de prompt zelf.
  const template = fs.readFileSync(PROMPT_FILE, 'utf-8').replace(/^<!--[\s\S]*?-->\s*/, '');
  return template
    .replaceAll('{{MODEL_OF_INTEREST}}', params.model)
    .replaceAll('{{LANGUAGE_PREFERENCE}}', LANGUAGE_LABELS[params.language])
    .replaceAll('{{PHONE_NUMBER}}', params.phoneNumber);
}

/**
 * Zet de meertalige prompt om naar een strikt Nederlandstalige variant:
 * de taalsectie wordt vervangen door een harde "alleen Nederlands"-regel en
 * de Engelse/Deense openingszinnen en voorbeeldantwoorden worden verwijderd.
 * Handig als taaldetectie in de praktijk voor verwarring zorgt.
 */
export function makeDutchOnly(prompt: string): string {
  const topRule =
    'BELANGRIJKSTE REGELS — LEES DIT EERST:\n' +
    '1. Dit hele gesprek voer je UITSLUITEND in het Nederlands. Elke zin ' +
    'die je uitspreekt is Nederlands, wat je ook denkt te horen. Merk je ' +
    'dat je per ongeluk een woord of zin in een andere taal zei, schakel ' +
    'dan onmiddellijk en zonder toelichting terug naar het Nederlands.\n' +
    '2. Schrijf alle getallen, bedragen en tijden voluit in Nederlandse ' +
    "woorden ('vijftig kilometer', 'tien uur') en vermijd Engelse woorden " +
    '— de stem slaat anders om naar een Amerikaans accent.\n' +
    '3. Maximaal twee korte zinnen en één vraag per beurt; som nooit ' +
    'meerdere opties op in één vraag.\n\n';
  const dutchOnlySection = `# Taal en gespreksstijl

Dit gesprek voer je UITSLUITEND in het Nederlands, van begin tot eind.

- Spreek vriendelijk, professioneel en in verzorgd ABN. Spreek de prospect
  aan met "u".
- Wissel NOOIT van taal, ook niet als de prospect in een andere taal
  antwoordt of daarom vraagt.
- Antwoordt de prospect in een andere taal, zeg dan vriendelijk in het
  Nederlands: "Excuses, ik kan dit gesprek alleen in het Nederlands voeren.
  Als u dat prettiger vindt, laat ik een collega u terugbellen." Kan het
  gesprek niet in het Nederlands verder, rond dan vriendelijk af.

Algemene spreekstijl:
- Dit is een telefoongesprek: korte, natuurlijke zinnen. Geen lijstjes,
  geen jargon, geen lange monologen.
- Maximaal twee korte zinnen en hoogstens één vraag per beurt. Ook de
  eindsamenvatting knip je op in korte zinnen.
- Getallen en tijden voluit uitspreken.
- Laat ruimte voor de prospect; onderbreek niet.
- Als de prospect aangeeft dat het niet gelegen komt: bied aan op een beter
  moment terug te bellen, noteer dat moment en rond vriendelijk af.

`;
  // Vervang de meertalige taalsectie.
  let out = prompt.replace(/# Talen en gespreksstijl[\s\S]*?(?=# Openingszinnen)/, dutchOnlySection);
  // Verwijs in de stem-sectie niet naar Engels/Deens.
  out = out.replace(/- Spreek je Engels of Deens[\s\S]*?moedertaalspreker\.\n/, '');
  // Alleen de Nederlandse openingszin behouden.
  out = out.replace(/\*\*Engels:\*\*[\s\S]*?(?=# Voorbeeldantwoorden)/, '');
  out = out.replace(/\(Pas "goedemiddag[^)]*\)\s*/, '');
  // Engelse en Deense voorbeeldantwoorden verwijderen.
  out = out.replace(/\*\*(Price\/discount \(EN\)|Pris\/rabat \(DA\)|Delivery time \(EN\)|Leveringstid \(DA\)):\*\*\n"[\s\S]*?"\n\n/g, '');
  out = out.replace(/\(Gebruik in het Engels en Deens[\s\S]*?\)\n/, '');
  // Verwijzing naar gesprekstaal in het rapportschema neutraliseren.
  out = out.replace(/- taal van het gesprek \(nl \/ en \/ da\)/, '- taal van het gesprek: altijd nl');
  return topRule + out;
}

/**
 * Openingszin per taal; bij 'auto' openen we in het Nederlands.
 * Maximaal ~7 seconden spreektijd: het OpenAI-realtime-model kapt vaste
 * openingen vanaf ~8 seconden halverwege af. De agent licht de reden van
 * het gesprek verder toe in de tweede beurt (zie agent-prompt.md).
 */
export function firstMessage(model: string, language: Language): string {
  switch (language) {
    case 'en':
      return (
        'Good afternoon, you are speaking with the phone assistant of Borst Automotive. ' +
        `I'm calling about your interest in the ${model}. Is this a good time?`
      );
    case 'da':
      return (
        'Goddag, du taler med telefonassistenten hos Borst Automotive. ' +
        `Jeg ringer angående din interesse for ${model}. Passer det nu?`
      );
    case 'nl':
    case 'auto':
    default:
      return (
        'Goedemiddag, u spreekt met de telefonische assistent van Borst Automotive. ' +
        `Ik bel over uw interesse in de ${model}. Komt het gelegen?`
      );
  }
}

/**
 * JSON Schema van de save_lead_report tool die de voice agent aanroept.
 * Dit schema wordt als function tool aan het (realtime) model meegegeven
 * en spiegelt het leadrapport in de mock database.
 */
export const saveLeadReportTool = {
  type: 'function' as const,
  function: {
    name: 'save_lead_report',
    description:
      'Sla het leadrapport op aan het einde van het gesprek. Roep deze tool precies één keer per gesprek aan, vlak voor het beëindigen van de call. Vul alleen in wat daadwerkelijk besproken is; gebruik null of lege lijsten voor onbekende velden.',
    parameters: {
      type: 'object',
      properties: {
        language: {
          type: ['string', 'null'],
          enum: ['nl', 'en', 'da', null],
          description: 'Taal waarin het gesprek (grotendeels) gevoerd is.',
        },
        appointment_requested: {
          type: ['boolean', 'null'],
          description: 'Wil de prospect een showroomafspraak?',
        },
        preferred_moments: {
          type: 'array',
          items: { type: 'string' },
          description:
            'Voorkeursmoment(en) voor de afspraak, zo concreet mogelijk, bijv. "zaterdag rond 11:00" of "donderdagmiddag".',
        },
        appointment_type: {
          type: ['string', 'null'],
          enum: ['proefrit', 'offerte', 'taxatie', 'combinatie', null],
          description: 'Type afspraak.',
        },
        summary: {
          type: ['string', 'null'],
          description: 'Feitelijke samenvatting van het gesprek in het Nederlands (2–5 zinnen).',
        },
        usage: {
          type: ['string', 'null'],
          description: 'Gebruikssituatie: privé/zakelijk, gezin, woon-werk, caravan, enz.',
        },
        desired_options: {
          type: 'array',
          items: { type: 'string' },
          description: 'Gewenste opties/accessoires, bijv. automaat, trekhaak, navigatie.',
        },
        fuel_preference: {
          type: ['string', 'null'],
          description: 'Gewenste brandstof/aandrijving, of "nog open".',
        },
        buy_or_lease: {
          type: ['string', 'null'],
          enum: ['kopen', 'leasen', null],
        },
        business_or_private: {
          type: ['string', 'null'],
          enum: ['zakelijk', 'particulier', null],
        },
        trade_in: {
          type: ['boolean', 'null'],
          description: 'Is er een auto om in te ruilen?',
        },
        trade_in_license_plate: {
          type: ['string', 'null'],
          description: 'Kenteken van het inruilvoertuig, indien genoemd.',
        },
        trade_in_mileage: {
          type: ['string', 'null'],
          description: 'Globale kilometerstand van het inruilvoertuig, indien genoemd.',
        },
        objections: {
          type: 'array',
          items: { type: 'string' },
          description: 'Belangrijkste bezwaren, bijv. prijs, levertijd, twijfel tussen modellen.',
        },
        lead_score: {
          type: ['integer', 'null'],
          minimum: 1,
          maximum: 5,
          description: 'Koopintentie/leadscore van 1 (geen interesse) tot 5 (afspraak, hoge intentie).',
        },
        recommended_next_step: {
          type: ['string', 'null'],
          description: 'Eén concrete aanbevolen vervolgstap voor de verkoper.',
        },
        notes: {
          type: ['string', 'null'],
          description: 'Aanvullende gespreksnotities.',
        },
      },
      required: ['appointment_requested', 'summary', 'lead_score'],
    },
  },
};
