/**
 * Losse test-call zonder draaiende webserver/webhook.
 *
 * Gebruik:
 *   npx tsx scripts/test-call.ts --to +31612345678 --model "Volkswagen Tiguan" [--lang nl|en|da|auto]
 *
 * Omdat er in deze testmodus geen publieke webhook-URL is, wordt de
 * save_lead_report-tool uit de prompt gehaald en sluit de agent af met een
 * mondelinge samenvatting. Transcript en opname zijn terug te zien in het
 * Vapi-dashboard onder Call Logs, of op te halen via scripts/call-status.ts.
 */
import { config } from '../src/config';
import { buildAgentPrompt, firstMessage } from '../src/prompts/automotive-agent';
import { Language } from '../src/types';

function arg(name: string): string | undefined {
  const i = process.argv.indexOf(`--${name}`);
  return i > -1 ? process.argv[i + 1] : undefined;
}

const to = arg('to');
const model = arg('model') ?? 'Volkswagen Tiguan';
const lang = (arg('lang') ?? 'auto') as Language;

if (!to || !/^\+\d{8,15}$/.test(to)) {
  console.error('Geef een telefoonnummer op in internationaal formaat, bijv. --to +31612345678');
  process.exit(1);
}
if (!config.vapi.apiKey || !config.vapi.phoneNumberId) {
  console.error('VAPI_API_KEY en VAPI_PHONE_NUMBER_ID moeten in .env staan.');
  process.exit(1);
}

// Prompt opbouwen en de tool-sectie vervangen door een mondelinge afsluiting,
// omdat er in deze losse test geen webhook is die tool calls kan ontvangen.
let prompt = buildAgentPrompt({ model, language: lang, phoneNumber: to });
prompt = prompt.replace(/# Tool-instructies[\s\S]*$/m, '').trim();
prompt +=
  '\n\n# Testmodus\n\n' +
  'De tool save_lead_report is in dit gesprek NIET beschikbaar. Sla het ' +
  'opslaan van het leadrapport over en sluit in plaats daarvan af met de ' +
  'korte mondelinge samenvatting uit de afsluitflow.';

async function main() {
  const payload = {
    phoneNumberId: config.vapi.phoneNumberId,
    customer: { number: to },
    assistant: {
      name: 'Borst Automotive Leadassistent (test)',
      firstMessage: firstMessage(model, lang),
      model: {
        provider: 'openai',
        model: config.openai.realtimeModel,
        messages: [{ role: 'system', content: prompt }],
      },
      voice: { provider: 'openai', voiceId: config.openai.realtimeVoice },
      stopSpeakingPlan: { numWords: 2, voiceSeconds: 0.4, backoffSeconds: 1 },
      startSpeakingPlan: { waitSeconds: 0.5 },
      maxDurationSeconds: 600,
    },
    metadata: { source: 'test-call-script', model, lang },
  };

  const res = await fetch(`${config.vapi.baseUrl}/call`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${config.vapi.apiKey}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(payload),
  });

  const body = await res.text();
  if (!res.ok) {
    console.error(`Vapi weigerde de call (HTTP ${res.status}):\n${body}`);
    process.exit(1);
  }
  const call = JSON.parse(body);
  console.log('Call gestart!');
  console.log('  callId :', call.id);
  console.log('  status :', call.status);
  console.log('  naar   :', to, `(${model}, taal: ${lang})`);
  console.log('\nStatus/transcript ophalen: npx tsx scripts/call-status.ts', call.id);
}

main().catch((err) => {
  console.error('Call starten mislukt:', err.message ?? err);
  process.exit(1);
});
