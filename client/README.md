# StockSense AI (Client)

Production-ready, mobile-first React app for an Indian stock market platform.

## Tech stack

- React + TypeScript (Vite)
- Tailwind CSS (dark fintech UI)
- React Router
- React Query
- Recharts

## Getting started

Install dependencies:

```bash
npm install
```

Run dev server:

```bash
npm run dev
```

Build for production:

```bash
npm run build
```

## API

Base URL is **`/api`** (same-origin).

### Dev proxy (optional)

If your .NET backend runs on a different port during development, set:

```bash
VITE_API_PROXY_TARGET=http://localhost:5000
```

`vite.config.ts` will proxy `/api/*` to that target in dev.
