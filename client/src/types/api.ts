// ─── Stock Details ───────────────────────────────────────────────────────────

export type NewsItem = {
  headline: string;
  summary: string;
  source: string;
  url: string;
  publishedAt: string;
};

export type StockDetailsResponse = {
  companyName: string;
  symbol: string;
  nsePrice: number;
  bsePrice: number;
  lastUpdated: string;
  news: NewsItem[];
};

// ─── Stock History ──────────────────────────────────────────────────────────────

export type PricePoint = {
  Date: string;
  Close: number;
};

export type StockHistoryResponse = {
  symbol: string;
  range: string;
  data: PricePoint[];
};

// ─── Trending / Market ──────────────────────────────────────────────────────────

export type TrendingStock = {
  symbol: string;
  companyName: string;
  price: number;
  change: number;
  changePercent: number;
};

export type TrendingStocksResponse = {
  topGainers: TrendingStock[];
  topLosers: TrendingStock[];
};

export type MarketIndex = {
  companyName: string;
  symbol: string;
  nsePrice: number;
  bsePrice: number;
  lastUpdated: string;
  change: number;
  changePercent: number;
};

export type MarketOverviewResponse =
  | {
      nifty?: MarketIndex;
      sensex?: MarketIndex;
      banknifty?: MarketIndex;
    }
  | Record<string, unknown>;

export type TopMoversResponse =
  | {
      topGainers?: TrendingStock[];
      topLosers?: TrendingStock[];
    }
  | Record<string, unknown>;

export type CompareStockResponseItem = {
  Ticker: string;
  Details: StockDetailsResponse;
};

// ─── Watchlist ──────────────────────────────────────────────────────────────────

export type WatchlistItem = { symbol: string } | string;

// ─── Opportunity Scanner ────────────────────────────────────────────────────────

export type ScoreBreakdown = {
  momentumScore: number;
  trendScore: number;
  volumeScore: number;
  riskScore: number;
};

export type StockMetrics = {
  stockId: number;
  symbol: string;
  companyName: string;
  exchange: string;
  latestClose: number;
  latestTimestamp: string;
  oneMonthReturnPct: number | null;
  threeMonthReturnPct: number | null;
  sixMonthReturnPct: number | null;
  sma20: number | null;
  sma50: number | null;
  sma200: number | null;
  distanceFromSma50Pct: number | null;
  distanceFromSma200Pct: number | null;
  sma50VsSma200Pct: number | null;
  currentVolume: number;
  averageVolume20: number | null;
  volumeRatio: number | null;
  averageDailyTradedValue: number | null;
  annualizedVolatilityPct: number | null;
};

export type OpportunityResult = {
  rank: number;
  stockId: number;
  symbol: string;
  companyName: string;
  exchange: string;
  latestClose: number;
  latestTimestamp: string;
  opportunityScore: number;
  overextensionPenalty: number;
  scores: ScoreBreakdown;
  signals: string[];
  metrics: StockMetrics;
};

export type ExplainOpportunityResponse = {
  summary: string;
  strengths: string[];
  watchPoints: string[];
  overall: string;
};
