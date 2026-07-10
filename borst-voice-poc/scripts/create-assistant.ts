/**
 * Maakt (of vervangt) de vaste "Borst Automotive Leadassistent" in het
 * Vapi-account, zodat oefencalls ook rechtstreeks vanuit het Vapi-dashboard
 * gestart kunnen worden (Phone Numbers → Make outbound call), met een vrij
 * in te vullen telefoonnummer.
 *
 * Gebruik:
 *   npx tsx scripts/create-assistant.ts [--model "Volkswagen Tiguan"] [--nl-only] [--stack pipeline|realtime]
 *
 * --nl-only: de agent spreekt uitsluitend Nederlands (geen taaldetectie),
 * en de spraakherkenning staat vast op Nederlands.
 *
 * --stack pipeline (standaard): GPT-4o als taalmodel + ElevenLabs als stem.
 *   Natuurlijk Nederlands accent, geen afgebroken zinnen, geen spontane
 *   taalwissels in de audio.
 * --stack realtime: OpenAI gpt-realtime-2 speech-to-speech (lagere latency,
 *   maar kapt zinnen soms af en spreekt Nederlands met accent).
 */
import { config } from '../src/config';
import { buildAgentPrompt, firstMessage, makeDutchOnly } from '../src/prompts/automotive-agent';

const NAME = 'Borst Automotive Leadassistent';

function arg(name: string): string | undefined {
  const i = process.argv.indexOf(`--${name}`);
  return i > -1 ? process.argv[i + 1] : undefined;
}

const carModel = arg('model') ?? 'Volkswagen Tiguan';
const nlOnly = process.argv.includes('--nl-only');
const stack = (arg('stack') ?? 'pipeline') as 'pipeline' | 'realtime';

// Testvariant van de prompt: zonder tool-sectie (er is geen webhook die
// tool calls kan ontvangen zolang de app niet gehost is).
let prompt = buildAgentPrompt({
  model: carModel,
  language: nlOnly ? 'nl' : 'auto',
  phoneNumber: 'het nummer waarop we de prospect nu bellen',
});
if (nlOnly) prompt = makeDutchOnly(prompt);
prompt = prompt.replace(/# Tool-instructies[\s\S]*$/m, '').trim();
prompt +=
  '\n\n# Testmodus\n\n' +
  'De tool save_lead_report is in dit gesprek NIET beschikbaar. Sla het ' +
  'opslaan van het leadrapport over en sluit in plaats daarvan af met de ' +
  'korte mondelinge samenvatting uit de afsluitflow.';

const assistantConfig = {
  name: NAME,
  firstMessage: firstMessage(carModel, nlOnly ? 'nl' : 'auto'),
  model: {
    provider: 'openai',
    model: stack === 'realtime' ? config.openai.realtimeModel : config.pipeline.llmModel,
    messages: [{ role: 'system', content: prompt }],
  },
  voice:
    stack === 'realtime'
      ? { provider: 'openai', voiceId: config.openai.realtimeVoice }
      : {
          provider: '11labs',
          voiceId: config.elevenlabs.voiceId,
          model: config.elevenlabs.model,
        },
  // Bij --nl-only staat ook de spraakherkenning vast op Nederlands, zodat
  // de agent Nederlandse sprekers niet per ongeluk als Engels/Deens verstaat.
  ...(nlOnly
    ? { transcriber: { provider: 'deepgram', model: 'nova-2', language: 'nl' } }
    : {}),
  // Onderbrekingsgevoeligheid: standaard stopt de agent bij elk geluid met
  // praten (ook echo/achtergrondgeluid). Met numWords: 2 stopt ze pas als de
  // beller echt minimaal twee woorden zegt.
  stopSpeakingPlan: { numWords: 2, voiceSeconds: 0.4, backoffSeconds: 1 },
  startSpeakingPlan: { waitSeconds: 0.5 },
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
  console.log('  stem :', assistant.voice?.provider, assistant.voice?.voiceId);
}

main().catch((err) => {
  console.error('Mislukt:', err.message ?? err);
  process.exit(1);
});
