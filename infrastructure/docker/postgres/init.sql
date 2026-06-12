-- Enable pgvector extension for semantic search
CREATE EXTENSION IF NOT EXISTS vector;
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Create revenue schema
CREATE SCHEMA IF NOT EXISTS revenue;

-- Set default search path
ALTER DATABASE revenue_intelligence SET search_path TO revenue, public;

-- Grant permissions
GRANT ALL ON SCHEMA revenue TO postgres;
