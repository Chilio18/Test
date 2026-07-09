/* Borst Automotive Voice PoC — eenvoudige vanilla JS frontend. */

const STATUS_LABELS = {
  not_called: 'Nog niet gebeld',
  queued: 'In wachtrij',
  in_progress: 'Bezig',
  completed: 'Afgerond',
  failed: 'Mislukt',
};

const LANGUAGE_LABELS = { auto: 'Auto-detect', nl: 'Nederlands', en: 'Engels', da: 'Deens' };

const els = {
  form: document.getElementById('call-form'),
  button: document.getElementById('call-button'),
  message: document.getElementById('form-message'),
  list: document.getElementById('lead-list'),
  detailCard: document.getElementById('detail-card'),
  detail: document.getElementById('lead-detail'),
  closeDetail: document.getElementById('close-detail'),
  modeBanner: document.getElementById('mode-banner'),
};

let selectedLeadId = null;

async function api(path, options) {
  const res = await fetch(path, options);
  const body = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(body.error || `Fout (${res.status})`);
  return body;
}

async function loadHealth() {
  try {
    const health = await api('/api/health');
    if (health.mockMode) {
      els.modeBanner.textContent =
        '🧪 Mock mode: calls worden gesimuleerd (geen Vapi-configuratie gevonden)';
      els.modeBanner.classList.remove('hidden');
    }
  } catch { /* niet kritisch */ }
}

async function refreshLeads() {
  try {
    const leads = await api('/api/leads');
    renderLeads(leads);
    if (selectedLeadId) {
      const current = leads.find((l) => l.id === selectedLeadId);
      if (current) renderDetail(current);
    }
  } catch (err) {
    els.list.innerHTML = `<p class="muted">Leads laden mislukt: ${escapeHtml(err.message)}</p>`;
  }
}

function renderLeads(leads) {
  if (!leads.length) {
    els.list.innerHTML = '<p class="muted">Nog geen leads. Voer hierboven een lead in en klik op “Bel lead”.</p>';
    return;
  }
  els.list.innerHTML = '';
  for (const lead of leads) {
    const row = document.createElement('div');
    row.className = 'lead-row';
    row.innerHTML = `
      <div class="lead-main">
        <div class="lead-model">${escapeHtml(lead.model)}</div>
        <div class="lead-phone">${escapeHtml(lead.phoneNumber)} · ${LANGUAGE_LABELS[lead.language] || lead.language}</div>
      </div>
      <div class="lead-meta">
        ${lead.report?.leadScore ? `<div class="score">Score ${lead.report.leadScore}/5</div>` : ''}
        <div>${formatDate(lead.updatedAt)}</div>
      </div>
      <span class="badge ${lead.status}">${STATUS_LABELS[lead.status] || lead.status}</span>
    `;
    row.addEventListener('click', () => {
      selectedLeadId = lead.id;
      renderDetail(lead);
    });
    els.list.appendChild(row);
  }
}

function renderDetail(lead) {
  els.detailCard.classList.remove('hidden');
  const r = lead.report;

  const head = `
    <p>
      <strong>${escapeHtml(lead.model)}</strong> · ${escapeHtml(lead.phoneNumber)}
      · <span class="badge ${lead.status}">${STATUS_LABELS[lead.status] || lead.status}</span>
    </p>
  `;

  if (!r) {
    els.detail.innerHTML = head + '<p class="muted">Nog geen leadrapport beschikbaar. Het rapport verschijnt hier zodra het gesprek is afgerond.</p>';
    return;
  }

  const items = [
    ['Taal gesprek', r.language ? LANGUAGE_LABELS[r.language] : null],
    ['Afspraak gewenst', fmtBool(r.appointmentRequested)],
    ['Voorkeursmoment(en)', r.preferredMoments?.length ? r.preferredMoments.join(', ') : null],
    ['Afspraaktype', r.appointmentType],
    ['Gebruikssituatie', r.usage],
    ['Gewenste opties', r.desiredOptions?.length ? r.desiredOptions.join(', ') : null],
    ['Brandstof/aandrijving', r.fuelPreference],
    ['Kopen/leasen', r.buyOrLease],
    ['Zakelijk/particulier', r.businessOrPrivate],
    ['Inruil', fmtBool(r.tradeIn)],
    ['Inruil kenteken', r.tradeInLicensePlate],
    ['Inruil kilometerstand', r.tradeInMileage],
    ['Belangrijkste bezwaren', r.objections?.length ? r.objections.join(', ') : null],
    ['Leadscore', r.leadScore ? `${r.leadScore} / 5` : null],
    ['Aanbevolen vervolgstap', r.recommendedNextStep],
  ];

  const grid = items
    .map(([k, v]) => `
      <div class="report-item">
        <div class="k">${k}</div>
        <div class="v">${v ? escapeHtml(String(v)) : '<span class="muted">—</span>'}</div>
      </div>`)
    .join('');

  const summary = r.summary
    ? `<div class="summary-block"><h3>Samenvatting</h3><p>${escapeHtml(r.summary)}</p></div>`
    : '';
  const transcript = r.transcript
    ? `<div class="transcript-block"><h3>Transcript / gespreksnotities</h3><pre>${escapeHtml(r.transcript)}</pre></div>`
    : '';

  els.detail.innerHTML = head + `<div class="report-grid">${grid}</div>` + summary + transcript;
}

els.closeDetail.addEventListener('click', () => {
  selectedLeadId = null;
  els.detailCard.classList.add('hidden');
});

els.form.addEventListener('submit', async (e) => {
  e.preventDefault();
  const data = new FormData(els.form);
  els.button.disabled = true;
  setMessage('Call wordt gestart…');
  try {
    const result = await api('/api/calls', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        phoneNumber: data.get('phoneNumber'),
        model: data.get('model'),
        language: data.get('language'),
      }),
    });
    setMessage(
      result.provider === 'mock'
        ? 'Gesimuleerde call gestart (mock mode). Volg de status hieronder.'
        : 'Call gestart via Vapi. Volg de status hieronder.',
      'ok'
    );
    els.form.reset();
    selectedLeadId = result.lead.id;
    await refreshLeads();
  } catch (err) {
    setMessage(err.message, 'error');
  } finally {
    els.button.disabled = false;
  }
});

function setMessage(text, kind) {
  els.message.textContent = text;
  els.message.className = 'form-message' + (kind ? ` ${kind}` : '');
}

function fmtBool(v) {
  if (v === true) return 'Ja';
  if (v === false) return 'Nee';
  return null;
}

function formatDate(iso) {
  try {
    return new Date(iso).toLocaleString('nl-NL', {
      day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit',
    });
  } catch {
    return iso;
  }
}

function escapeHtml(s) {
  return String(s)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;');
}

loadHealth();
refreshLeads();
setInterval(refreshLeads, 3000);
