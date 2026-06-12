# Revenue Intelligence Platform — Architecture

## Overview

UnameIT Revenue Intelligence is an AI-powered sales conversation analytics platform built as a multi-tenant SaaS product. It records, transcribes, and analyzes customer conversations to improve sales performance and CRM data quality.

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Nuxt 3, Vue 3, TypeScript, Tailwind CSS, Pinia, Zod |
| Backend API | .NET 8 Web API, Clean Architecture, CQRS with MediatR |
| Database | PostgreSQL 16 with pgvector extension |
| Cache / Queue | Redis (StackExchange.Redis, Hangfire) |
| Storage | S3-compatible (AWS S3 / MinIO for local dev) |
| AI | OpenAI GPT-4o / Anthropic Claude / Azure OpenAI |
| Speech | OpenAI Whisper / Azure Speech Services |
| Auth | OIDC / OAuth2 (Microsoft Entra ID, Google) |
| Logging | Serilog → Seq |
| Real-time | SignalR |

## Solution Structure

```
backend/
  src/
    UnameIT.RevenueIntelligence.Domain/      ← Entities, Value Objects, Domain Events
    UnameIT.RevenueIntelligence.Application/ ← CQRS Commands/Queries, Interfaces, DTOs
    UnameIT.RevenueIntelligence.Infrastructure/ ← EF Core, AI/CRM/Speech adapters, Redis, S3
    UnameIT.RevenueIntelligence.API/         ← Controllers, Middleware, SignalR, Program.cs
  tests/
    *.Domain.Tests/
    *.Application.Tests/
    *.Integration.Tests/

frontend/
  app/
    pages/       ← File-based routing
    components/  ← Feature-grouped components
    composables/ ← Reusable Vue composition functions
    stores/      ← Pinia state management
    types/       ← TypeScript type definitions
    layouts/     ← Default and auth layouts
```

## Key Architectural Decisions

### Multi-Tenancy
Every entity extends `TenantEntity` with a `TenantId`. EF Core global query filters enforce tenant isolation at the data layer. Tenant context is resolved from JWT claims (`tenant_id` claim) via `ICurrentTenant`.

### AI Abstraction Layer
All AI calls go through `IAIProvider`. Implementations: `OpenAIProvider`, `AnthropicProvider`, `AzureOpenAIProvider`, `MockAIProvider`. Switch provider via `appsettings.json: AI.Provider`.

### CRM Adapter Layer
All CRM operations go through `ICrmProvider`. Implementations: `ZohoCrmProvider`, `InternalDotNetCrmProvider`, `MockCrmProvider`. Business logic never touches CRM APIs directly.

### Speech Abstraction
All transcription goes through `ISpeechProvider`. Implementations: `WhisperSpeechProvider`, `AzureSpeechProvider`, `MockSpeechProvider`.

### Call Processing Pipeline
1. Upload → Store recording in S3
2. Hangfire background job picks up the recording
3. Speech provider transcribes audio → segments stored in DB
4. AI provider analyzes transcript → summaries, insights, action items, deal risk, coaching
5. Embeddings indexed to pgvector for RAG search
6. SignalR notification sent to owner
7. Optional: CRM sync

## Security & Compliance

- OIDC/OAuth2 authentication with JWT validation
- Role-based authorization: PlatformAdmin, OrganizationAdmin, SalesManager, SalesRepresentative, Viewer
- AES-256 encryption for sensitive fields (API keys, OAuth tokens)
- HTTPS enforced
- Audit logging for all sensitive operations
- GDPR: consent tracking per call, configurable data retention policies
- S3 server-side encryption at rest

## Local Development

```bash
# Start infrastructure
docker compose -f docker-compose.dev.yml up -d

# Backend
cd backend
dotnet run --project src/UnameIT.RevenueIntelligence.API

# Frontend
cd frontend
npm install
npm run dev
```
