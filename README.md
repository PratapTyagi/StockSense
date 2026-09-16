# StockSenseAI

StockSenseAI is a full-stack stock analysis platform for the Indian stock market.

It combines market data, historical analysis, stock screening, watchlists, and AI-generated explanations into a single application.

The main focus of the project is the **Opportunity Scanner**, which evaluates 3,000+ equities and identifies the top opportunities based on a defined set of criteria.

---

## Features

- Market overview
- Stock details and historical analysis
- Opportunity Scanner across 3,000+ equities
- Top gainers and losers
- Watchlist
- AI-generated stock explanations
- Redis caching
- Automated market-data synchronization
- Docker Compose based architecture

---

## Architecture

StockSenseAI is split into three main application components:

- **Frontend** — React application used by the user
- **Backend** — Handles application APIs and business logic
- **StockDataService** — Background service responsible for collecting and maintaining market data

Nginx sits between the frontend and backend as a reverse proxy.

### User Request Flow

```text
┌────────────────────┐
│      Browser       │
│   React + Vite     │
└─────────┬──────────┘
          │
          │ Same Domain
          ▼
┌────────────────────┐
│       Nginx        │
│   Reverse Proxy    │
└─────────┬──────────┘
          │
          │ Internal routing
          ▼
┌────────────────────┐
│      Backend       │
│      .NET 10       │
│                    │
│  Market Overview   │
│  Opportunity       │
│  Stock Details     │
│  AI Explanation    │
│  Watchlist         │
└────────┬─────┬─────┘
         │     │
         │     └──────────────┐
         ▼                    ▼
┌────────────────┐     ┌──────────────┐
│  StockData DB  │     │    Redis     │
│   SQL Server   │     │    Cache     │
└────────────────┘     └──────────────┘
```

The frontend does **not** know the Backend's IP address or port.

For example, the frontend makes a request using the same domain:

```text
GET /api/stock/INFY
```

Nginx receives the request and forwards it internally to the Backend.

---

## Backend

The Backend is the main application service and contains the business logic used by the frontend.

### Market Overview

The Market Overview uses **RapidAPI** to fetch:

- Market indexes
- Top movers

### Opportunity Scanner

The Opportunity Scanner is the main differentiating feature of StockSenseAI.

It evaluates **3,000+ equities** based on a defined set of criteria and returns the top `N` opportunities.

The scanner processes the available market and historical information to calculate the opportunity score.

The score is an analytical ranking signal and is not a prediction of future returns or a buy/sell recommendation.

### Stock Details

The Stock Details endpoint provides information derived from historical stock data.

Frequently accessed derived information is stored in **Redis** to improve response performance.

### AI Explanation

The AI Explanation feature uses information derived from historical stock data and sends it to **Grok** to generate an explanation of the stock's strengths and weaknesses.

The AI is used to explain the existing analysis rather than replace the underlying scoring logic.

### Watchlist

The Watchlist allows a user to add stocks for easier access later.

---

## StockDataService

`StockDataService` is a background service responsible for collecting and maintaining market data.

It is **not part of the normal frontend request path**.

```text
             ┌──────────────────────┐
             │  StockDataService    │
             │       .NET 10        │
             │                      │
             │   Background Worker  │
             └──────────┬───────────┘
                        │
                        │ Every 2 days
                        ▼
                 ┌──────────────┐
                 │  Zerodha API │
                 └──────┬───────┘
                        │
                   Market Data
                        │
                        ▼
                 ┌────────────────┐
                 │  StockData DB  │
                 │   SQL Server   │
                 └────────────────┘
```

The worker periodically fetches:

- Stock information
- Historical stock data

The service uses a Zerodha endpoint as the market-data source.

StockDataService runs as a background service and has **no port exposed to the host**.

---

## Backend ↔ StockDataService

The Backend can communicate with StockDataService for internal operations.

One such operation is supplying the Zerodha `enctoken`.

```text
┌──────────────┐
│   Backend    │
└──────┬───────┘
       │
       │ POST /enctoken
       │
       │ Authorization / API Key
       ▼
┌──────────────────────┐
│  StockDataService    │
└──────────────────────┘
```

The `/enctoken` route requires authorization through API-key validation.

The frontend does not directly communicate with StockDataService.

---

## Security

StockDataService is an internal service.

### Network isolation

No StockDataService port is exposed to the host.

This means it is not directly accessible through the server's public IP.

### Authorization

Operations such as `/enctoken` require API-key validation.

The API key is kept on the server side and is not exposed to the frontend.

The communication model is:

```text
Internet
   │
   ▼
 Nginx
   │
   ▼
Backend
   │
   │ Authorized internal request
   ▼
StockDataService
```

---

## StockData Database

The StockData database uses **SQL Server**.

Database initialization is handled through the `stockDataDb` database project and its generated **DACPAC**.

```text
stockDataDb/
       │
       ▼
     DACPAC
       │
       ▼
  StockData DB
  SQL Server
```

SQL Server does not need to be publicly accessible.

Its port is not exposed to the host in the application architecture.

---

## Data Flow

There are two main data flows in the application.

### User-facing flow

```text
Browser
   ↓
Nginx
   ↓
Backend
   ↓
Redis / StockData DB
```

### Market-data flow

```text
Zerodha
   ↓
StockDataService
   ↓
StockData DB
   ↓
Backend
   ↓
Browser
```

This separation keeps market-data ingestion independent from the user-facing application APIs.

---

## Redis

Redis is used as a caching layer.

For stock details and other frequently accessed derived information:

```text
Backend
   │
   ▼
Redis
   │
   └── Cache miss
          │
          ▼
     StockData DB
```

SQL Server remains the persistent data store, while Redis is used to improve performance.

---

## Future Architecture

The current application is designed so that it can later become a user-specific dashboard.

The planned direction is to introduce:

- User-specific data
- Separate database/data separation where required
- User authorization
- User-specific dashboard information

The current architecture keeps the application and data-ingestion responsibilities separated, providing a foundation for these future changes.

---

## Technology Stack

### Frontend

- React
- TypeScript
- Vite
- Nginx

### Backend

- .NET 10
- REST APIs

### Database

- SQL Server
- DACPAC / SQL Database Project

### Cache

- Redis

### Market Data

- Zerodha
- RapidAPI

### AI

- Grok

### Infrastructure

- Docker
- Docker Compose

---

## Project Structure

```text
StockSenseAI/
│
├── client/
│   └── React + Vite frontend
│
├── backend/
│   ├── Backend.Domain/
│   ├── Backend.Application/
│   └── Backend.Infrastructure/
│
├── stockDataService/
│   └── Background market-data service
│
├── stockDataDb/
│   └── SQL Server database project
│
├── docker-compose.yml
│
└── .env.example
```

---

## Running Locally

### Prerequisites

- Docker Desktop
- Git
- SqlPackage

### Clone the repository

```bash
git clone <repository-url>
cd StockSenseAI
```

### Configure environment variables

Create `.env` from `.env.example`:

```bash
cp .env.example .env
```

Update the required configuration values.

**Never commit `.env` to source control.**

### Start the application

```bash
docker compose up -d --build
```

Check running containers:

```bash
docker compose ps
```

View logs:

```bash
docker compose logs -f
```

---

## Design Principles

### Clear separation of responsibilities

The Backend handles application logic, while StockDataService handles market-data ingestion.

### Frontend simplicity

The frontend communicates using the same domain and does not need to know internal Backend ports or IP addresses.

### Internal service isolation

StockDataService and SQL Server are not exposed directly to the host.

### Data-driven analysis

The Opportunity Scanner derives its results from market and historical data rather than relying on manually selected stocks.

### Simple infrastructure

Docker Compose provides the required container networking and isolation without introducing unnecessary infrastructure complexity.

---

## Disclaimer

StockSenseAI is a software project for analyzing and presenting market data.

Opportunity scores and AI explanations are analytical outputs and should not be interpreted as financial advice, guaranteed returns, or buy/sell recommendations.
