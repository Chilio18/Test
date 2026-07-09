import dotenv from 'dotenv';

dotenv.config();

const vapiApiKey = process.env.VAPI_API_KEY ?? '';

export const config = {
  port: Number(process.env.PORT ?? 3000),
  publicBaseUrl: process.env.PUBLIC_BASE_URL ?? '',

  vapi: {
    apiKey: vapiApiKey,
    phoneNumberId: process.env.VAPI_PHONE_NUMBER_ID ?? '',
    webhookSecret: process.env.VAPI_WEBHOOK_SECRET ?? '',
    baseUrl: 'https://api.vapi.ai',
  },

  // Voice/realtime model is configureerbaar via env vars.
  openai: {
    realtimeModel: process.env.OPENAI_REALTIME_MODEL ?? 'gpt-realtime-2',
    realtimeVoice: process.env.OPENAI_REALTIME_VOICE ?? 'marin',
  },

  // Zonder Vapi API key draait de PoC automatisch in mock mode.
  mockMode: process.env.MOCK_MODE === 'true' || !vapiApiKey,
};
