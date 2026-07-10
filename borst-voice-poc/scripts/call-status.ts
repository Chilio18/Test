/**
 * Status, samenvatting en transcript van een Vapi-call ophalen.
 *
 * Gebruik:
 *   npx tsx scripts/call-status.ts <callId>
 */
import { config } from '../src/config';

const callId = process.argv[2];
if (!callId) {
  console.error('Gebruik: npx tsx scripts/call-status.ts <callId>');
  process.exit(1);
}

async function main() {
  const res = await fetch(`${config.vapi.baseUrl}/call/${callId}`, {
    headers: { Authorization: `Bearer ${config.vapi.apiKey}` },
  });
  if (!res.ok) {
    console.error(`Ophalen mislukt (HTTP ${res.status}):`, await res.text());
    process.exit(1);
  }
  const call = (await res.json()) as any;
  console.log('status      :', call.status);
  console.log('endedReason :', call.endedReason ?? '-');
  console.log('gestart     :', call.startedAt ?? '-');
  console.log('beëindigd   :', call.endedAt ?? '-');
  console.log('kosten      :', call.cost != null ? `$${call.cost}` : '-');
  if (call.analysis?.summary) {
    console.log('\n── Samenvatting (Vapi) ──\n' + call.analysis.summary);
  }
  const transcript = call.transcript ?? call.artifact?.transcript;
  if (transcript) {
    console.log('\n── Transcript ──\n' + transcript);
  }
  if (call.artifact?.recordingUrl || call.recordingUrl) {
    console.log('\nOpname:', call.artifact?.recordingUrl ?? call.recordingUrl);
  }
}

main().catch((err) => {
  console.error('Fout:', err.message ?? err);
  process.exit(1);
});
