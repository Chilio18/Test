# Borst Automotive — Telefonische AI-leadassistent (PoC)

Proof-of-concept webapplicatie voor een multibrand telefonische AI-assistent
die autoleads opvolgt, kwalificeert en stuurt naar een showroomafspraak
(proefrit, offertegesprek, inruiltaxatie of een combinatie).

Flow: lead invoeren (telefoonnummer + model) → **“Bel lead”** → de voice
agent belt de prospect → na afloop staat er een volledig leadrapport in de
mock database, zichtbaar in de webinterface.

## Architectuurkeuze: Vapi + OpenAI gpt-realtime-2

We gebruiken **Vapi** als voice-orchestratielaag, met **OpenAI
`gpt-realtime-2`** (configureerbaar via `OPENAI_REALTIME_MODEL`) als
speech-to-speech model. Waarom Vapi en niet direct de OpenAI Realtime API:

- **Telefonie inbegrepen.** Vapi regelt outbound calls, nummers en
  SIP/PSTN. Direct met de OpenAI Realtime API zou je zelf een
  Twilio/SIP-koppeling, audio-bridging en gespreksbeheer moeten bouwen —
  precies het loodgieterswerk dat een PoC niet nodig heeft.
- **Gespreksmechanica out-of-the-box:** interruptie/barge-in,
  end-of-call-reports, transcripts, voicemail-detectie en webhooks.
- **Je hebt al een Vapi-account**, dus geen extra leverancier nodig.
- Het beste van beide: Vapi kan OpenAI-realtime-modellen als provider
  gebruiken, dus we draaien alsnog `gpt-realtime-2` als voice-model.

De OpenAI Realtime API direct gebruiken wordt pas interessant bij
productie-integratie in eigen software, als je volledige controle over de
audiopijplijn wilt en de telefoniekoppeling zelf beheert.

## Stack

- **Node.js + Express + TypeScript** — bewust een kleine, pragmatische stack
  (geen Next.js nodig: de UI is één statische pagina, de kern is de
  API/webhook-laag die later in eigen software wordt geïntegreerd).
- **Mock database:** JSON-file store (`data/db.json`), geïnitialiseerd met
  voorbeelddata uit `data/seed-leads.json`.
- **Frontend:** statische HTML/CSS/JS (geen build-stap), gepolld elke 3s.
- **Voice agent prompt:** [`agent-prompt.md`](./agent-prompt.md) — de
  volledige system prompt, los van de code aanpasbaar.

## Projectstructuur

```
borst-voice-poc/
├── agent-prompt.md              # Volledige system prompt van de voice agent
├── data/seed-leads.json         # Voorbeelddata (wordt db.json bij eerste start)
├── public/                      # Webinterface (index.html, app.js, styles.css)
└── src/
    ├── server.ts                # Express app + routes + static hosting
    ├── config.ts                # Env-configuratie (model, Vapi, mock mode)
    ├── db.ts                    # Mock database (JSON-file store)
    ├── types.ts                 # Lead, LeadReport, statussen
    ├── vapi.ts                  # Vapi client + transient assistant config
    ├── mockCall.ts              # Gesimuleerde calls (demo zonder Vapi)
    ├── prompts/automotive-agent.ts  # Prompt-loader + save_lead_report tool-schema
    └── routes/
        ├── leads.ts             # CRUD leads + leadrapport
        ├── calls.ts             # Call starten
        └── webhooks.ts          # Vapi webhook (status, tool-calls, report)
```

## Installatie & starten

Vereist: Node.js 20+.

```bash
cd borst-voice-poc
npm install
cp .env.example .env   # en vul je keys in (of laat MOCK_MODE=true staan)
npm run dev            # ontwikkelmodus met auto-reload
```

Open daarna <http://localhost:3000>.

Productie-build:

```bash
npm run build && npm start
```

### Mock mode (standaard)

Zonder `VAPI_API_KEY` (of met `MOCK_MODE=true`) simuleert de app de call:
status loopt van *In wachtrij* → *Bezig* → *Afgerond* en er wordt een
realistisch leadrapport gegenereerd. Zo is de volledige UI- en dataflow te
demonstreren zonder account of tunnel.

### Echte calls via Vapi

1. Zet in `.env`: `MOCK_MODE=false`, je `VAPI_API_KEY` en een
   `VAPI_PHONE_NUMBER_ID` (een nummer dat je in het Vapi-dashboard hebt
   aangemaakt/gekoppeld).
2. Zorg dat de app publiek bereikbaar is voor webhooks, bijv. met
   `ngrok http 3000`, en zet de tunnel-URL in `PUBLIC_BASE_URL`.
3. Optioneel: zet `VAPI_WEBHOOK_SECRET`; hetzelfde secret wordt per call in
   de assistant-config meegestuurd en bij binnenkomst gecontroleerd.

De assistant wordt per call als *transient assistant* meegestuurd
(prompt, model, tools), dus er hoeft niets in het Vapi-dashboard beheerd te
worden. Het realtime model en de stem zijn configureerbaar via
`OPENAI_REALTIME_MODEL` (default `gpt-realtime-2`) en
`OPENAI_REALTIME_VOICE`.

## API-endpoints

| Methode | Pad                     | Doel |
|--------:|-------------------------|------|
| `POST`  | `/api/leads`            | Lead aanmaken (`{ phoneNumber, model, language? }`) |
| `GET`   | `/api/leads`            | Leads ophalen (recentste eerst) |
| `GET`   | `/api/leads/:id`        | Eén lead incl. leadrapport |
| `POST`  | `/api/leads/:id/report` | Leadrapport (deels) opslaan — mock endpoint |
| `POST`  | `/api/calls`            | Call starten (`{ leadId }` óf `{ phoneNumber, model, language? }`) |
| `POST`  | `/api/webhooks/vapi`    | Vapi server messages: status-updates, tool-calls, end-of-call-report |
| `GET`   | `/api/health`           | Status + actieve modus/model |

Statussen van een lead: `not_called` (nog niet gebeld), `queued`,
`in_progress` (bezig), `completed` (afgerond), `failed` (mislukt).

## Hoe het leadrapport tot stand komt

1. De system prompt instrueert de agent om aan het einde van elk gesprek de
   function tool **`save_lead_report`** aan te roepen (schema in
   `src/prompts/automotive-agent.ts`).
2. Vapi stuurt die tool call naar `/api/webhooks/vapi`; de app mapt de
   argumenten naar het `LeadReport`-formaat en slaat ze op bij de lead.
3. Als vangnet bewaart de webhook bij het `end-of-call-report` ook het
   transcript en de Vapi-samenvatting, mocht de tool call uitblijven.

Het rapport bevat o.a.: afspraak gewenst, voorkeursmoment(en), afspraaktype,
samenvatting, gebruikssituatie, gewenste opties, brandstof, kopen/leasen,
zakelijk/particulier, inruil (kenteken + kilometerstand), bezwaren,
leadscore 1–5, aanbevolen vervolgstap en transcript.

## Talen

De agent ondersteunt Nederlands, Engels en Deens. Bij taalkeuze
*Auto-detect* opent de agent in het Nederlands en schakelt hij mee met de
taal van de prospect (ook bij wisselen tijdens het gesprek). Samenvattingen
in het leadrapport zijn altijd Nederlands.

## TODO's voor productie-integratie (eigen software Borst Automotive)

- [ ] `src/db.ts` vervangen door koppeling met eigen database/CRM/LMS
      (alle datatoegang loopt al via die ene module).
- [ ] Leads automatisch aanmaken vanuit formulier-/advertentieplatform-
      webhooks i.p.v. handmatige invoer.
- [ ] Webhook-beveiliging verzwaren: signature-verificatie en
      schema-validatie (bijv. zod) van Vapi-payloads.
- [ ] Opt-out/bel-me-niet-register en AVG-grondslag checken vóór outbound
      calls; opnames/transcripts conform bewaartermijnen opslaan.
- [ ] Retries, rate limiting en monitoring rond de Vapi API.
- [ ] Prompt-versiebeheer en A/B-testen van conversiestrategieën
      (`agent-prompt.md` is daar al op voorbereid).
- [ ] Afspraakbevestiging naar de verkoper pushen (mail/CRM-taak) op basis
      van `recommendedNextStep`.
- [ ] Authenticatie op de webinterface en API.
