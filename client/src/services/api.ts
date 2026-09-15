import type {
  CompareStockResponseItem,
  ExplainOpportunityResponse,
  MarketOverviewResponse,
  OpportunityResult,
  StockDetailsResponse,
  StockHistoryResponse,
  TopMoversResponse,
  TrendingStock,
  TrendingStocksResponse,
  WatchlistItem,
} from "../types/api";

export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`/api${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(init?.headers || {}),
    },
    ...init,
  });

  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new ApiError(text || `Request failed: ${res.status}`, res.status);
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}

export const api = {
  // ─── Opportunities ──────────────────────────────────────────────────────────
  opportunities: (params?: {
    top?: number;
    minScore?: number;
    exchange?: string;
  }) => {
    const searchParams = new URLSearchParams();
    if (params?.top) searchParams.set("top", String(params.top));
    if (params?.minScore) searchParams.set("minScore", String(params.minScore));
    if (params?.exchange) searchParams.set("exchange", params.exchange);
    const qs = searchParams.toString();
    return request<OpportunityResult[]>(`/opportunities${qs ? `?${qs}` : ""}`);
  },

  opportunityExplanation: (symbol: string) =>
    request<ExplainOpportunityResponse>(
      `/opportunities/${encodeURIComponent(symbol)}/explanation`,
    ),

  // ─── Stock Details ──────────────────────────────────────────────────────────
  stockDetails: (symbol: string) =>
    request<StockDetailsResponse>(`/stock?name=${encodeURIComponent(symbol)}`),

  stockHistory: (symbol: string, range: string) =>
    request<StockHistoryResponse>(
      `/stocks/${encodeURIComponent(symbol)}/history?range=${encodeURIComponent(range)}`,
    ),

  // ─── Trending / Market ──────────────────────────────────────────────────────
  trending: () => request<TrendingStocksResponse>(`/stock/trending`),

  compare: (symbols: string[]) =>
    request<CompareStockResponseItem[]>(
      `/stocks/compare?symbols=${encodeURIComponent(symbols.join(","))}`,
    ),

  batch: (symbols: string[]) =>
    request<TrendingStock[]>(`/stocks/batch`, {
      method: "POST",
      body: JSON.stringify({ symbols }),
    }),

  marketOverview: async (): Promise<MarketOverviewResponse> => {
    // API returns array: [{ ticker: "NIFTY", details: {...} }, ...]
    // Component expects: { nifty?: MarketIndex, sensex?: MarketIndex, banknifty?: MarketIndex }
    const items = await request<
      Array<{ ticker: string; details: Record<string, unknown> }>
    >(`/stock/compare?tickers=NIFTY,BSE,BANKNIFTY`);

    const tickerToKey: Record<string, string> = {
      NIFTY: "nifty",
      BSE: "sensex",
      BANKNIFTY: "banknifty",
    };

    const result: Record<string, unknown> = {};
    for (const item of items) {
      const key =
        tickerToKey[item.ticker?.toUpperCase?.()] ??
        item.ticker?.toLowerCase?.();
      if (key) {
        result[key] = item.details;
      }
    }
    return result as MarketOverviewResponse;
  },

  topMovers: () => request<TopMoversResponse>(`/stocks/top-movers`),

  // ─── Watchlist ──────────────────────────────────────────────────────────────
  watchlistGet: () => request<WatchlistItem[]>(`/watchlist`),

  watchlistAdd: (symbol: string) =>
    request<void>(`/watchlist`, {
      method: "POST",
      body: JSON.stringify({ symbol }),
    }),

  watchlistRemove: (symbol: string) =>
    request<void>(`/watchlist/${encodeURIComponent(symbol)}`, {
      method: "DELETE",
    }),
};
