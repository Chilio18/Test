import express from 'express';
import path from 'path';
import { config } from './config';
import { initDb } from './db';
import { leadsRouter } from './routes/leads';
import { callsRouter } from './routes/calls';
import { webhooksRouter } from './routes/webhooks';

initDb();

const app = express();
app.use(express.json({ limit: '2mb' }));

// API
app.use('/api/leads', leadsRouter);
app.use('/api/calls', callsRouter);
app.use('/api/webhooks', webhooksRouter);

app.get('/api/health', (_req, res) => {
  res.json({
    ok: true,
    mockMode: config.mockMode,
    realtimeModel: config.openai.realtimeModel,
  });
});

// Statische webinterface
app.use(express.static(path.join(process.cwd(), 'public')));

app.listen(config.port, () => {
  console.log(`Borst Automotive Voice PoC draait op http://localhost:${config.port}`);
  console.log(
    config.mockMode
      ? 'Mock mode actief: calls worden gesimuleerd (geen Vapi nodig).'
      : `Vapi actief, realtime model: ${config.openai.realtimeModel}`
  );
});
