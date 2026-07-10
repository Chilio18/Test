/**
 * Maakt (of vervangt) de vaste "Borst Automotive Leadassistent" in het
 * Vapi-account, zodat oefencalls ook rechtstreeks vanuit het Vapi-dashboard
 * gestart kunnen worden (Phone Numbers → Make outbound call), met een vrij
 * in te vullen telefoonnummer.
 *
 * Gebruik:
 *   npx tsx scripts/create-assistant.ts [--model "Volkswagen Tiguan"]
 */
import { config } from '../src/config';
import { buildAgentPrompt, firstMessage } from '../src/prompts/automotive-agent';

const NAME = 'Borst Automotive Leadassistent';

function arg(name: string): string | undefined {
  const i = process.argv.indexOf(`--${name}`);
  return i > -1 ? process.argv[i + 1] : undefined;
}

const carModel = arg('model') ?? 'Volkswagen Tiguan';

// Testvariant van de prompt: zonder tool-sectie (er is geen webhook die
// tool calls kan ontvangen zolang de app niet gehost is).
let prompt = buildAgentPrompt({
  model: carModel,
  language: 'auto',
  phoneNumber: 'het nummer waarop we de prospect nu bellen',
});
prompt = prompt.replace(/# Tool-instructies[\s\S]*$/m, '').trim();
prompt +=
  '\n\n# Testmodus\n\n' +
  'De tool save_lead_report is in dit gesprek NIET beschikbaar. Sla het ' +
  'opslaan van het leadrapport over en sluit in plaats daarvan af met de ' +
  'korte mondelinge samenvatting uit de afsluitflow.';

const assistantConfig = {
  name: NAME,
  firstMessage: firstMessage(carModel, 'auto'),
  model: {
    provider: 'openai',
    model: config.openai.realtimeModel,
    messages: [{ role: 'system', content: prompt }],
  },
  voice: { provider: 'openai', voiceId: config.openai.realtimeVoice },
  maxDurationSeconds: 600,
};

async function api(path: string, method: string, body?: unknown) {
  const res = await fetch(`${config.vapi.baseUrl}${path}`, {
    method,
    headers: {
      Authorization: `Bearer ${config.vapi.apiKey}`,
      'Content-Type': 'application/json',
    },
    body: body ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  if (!res.ok) throw new Error(`${method} ${path} → HTTP ${res.status}: ${text}`);
  return text ? JSON.parse(text) : null;
}

async function main() {
  const existing = (await api('/assistant?limit=100', 'GET')) as any[];
  const match = existing.find((a) => a.name === NAME);

  let assistant;
  if (match) {
    assistant = await api(`/assistant/${match.id}`, 'PATCH', assistantConfig);
    console.log(`Assistent bijgewerkt (model van interesse: ${carModel})`);
  } else {
    assistant = await api('/assistant', 'POST', assistantConfig);
    console.log(`Assistent aangemaakt (model van interesse: ${carModel})`);
  }
  console.log('  id   :', assistant.id);
  console.log('  naam :', assistant.name);
  console.log('  llm  :', assistant.model?.model);
}

main().catch((err) => {
  console.error('Mislukt:', err.message ?? err);
  process.exit(1);
});
