import type {
  CompareStockResponseItem,
  MarketOverviewResponse,
  StockDetailsResponse,
  StockHistoryResponse,
  TopMoversResponse,
  TrendingStocksResponse,
  TrendingStock,
  WatchlistItem,
} from '../types/api'
import { API_DEFAULT_URL } from '../constants'

export class ApiError extends Error {
  status: number
  constructor(message: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const url = new URL(`/api${path}`, import.meta.env.VITE_API_URL || API_DEFAULT_URL);
  const res = await fetch(url, {
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers || {}),
    },
    ...init,
  })

  if (!res.ok) {
    const text = await res.text().catch(() => '')
    throw new ApiError(text || `Request failed: ${res.status}`, res.status)
  }

  if (res.status === 204) return undefined as T
  return (await res.json()) as T
}

export const api = {
  stockDetails: (symbol: string) =>
    request<StockDetailsResponse>(`/stocks/${encodeURIComponent(symbol)}`),

  stockHistory: (symbol: string, range: string) =>
    request<StockHistoryResponse>(
      `/stocks/${encodeURIComponent(symbol)}/history?range=${encodeURIComponent(range)}`,
    ),

  trending: () => request<TrendingStocksResponse>(`/stock/trending`),

  compare: (symbols: string[]) =>
    request<CompareStockResponseItem[]>(
      `/stocks/compare?symbols=${encodeURIComponent(symbols.join(','))}`,
    ),

  batch: (symbols: string[]) =>
    request<TrendingStock[]>(`/stocks/batch`, {
      method: 'POST',
      body: JSON.stringify({ symbols }),
    }),

  marketOverview: () => request<MarketOverviewResponse>(`/stock/compare?tickers=NIFTY,BSE,BANKNIFTY`),

  topMovers: () => request<TopMoversResponse>(`/stocks/top-movers`),

  watchlistGet: () => request<WatchlistItem[]>(`/watchlist`),
  watchlistAdd: (symbol: string) =>
    request<void>(`/watchlist`, {
      method: 'POST',
      body: JSON.stringify({ symbol }),
    }),
  watchlistRemove: (symbol: string) =>
    request<void>(`/watchlist/${encodeURIComponent(symbol)}`, {
      method: 'DELETE',
    }),

  aiSummary: (symbol: string) =>
    request<unknown>(`/stocks/${encodeURIComponent(symbol)}/ai-summary`),
}

