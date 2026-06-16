export type NewsItem = {
  headline: string
  summary: string
  source: string
  url: string
  publishedAt: string
}

export type StockDetailsResponse = {
  companyName: string
  symbol: string
  nsePrice: number
  bsePrice: number
  lastUpdated: string
  news: NewsItem[]
}

export type PricePoint = {
  Date: string
  Close: number
}

export type StockHistoryResponse = {
  symbol: string
  range: string
  data: PricePoint[]
}

export type TrendingStock = {
  symbol: string
  price: number
  change: number
  changePercent: number
}

export type TrendingStocksResponse = {
  topGainers: TrendingStock[]
  topLosers: TrendingStock[]
}

export type MarketIndex = {
  companyName: string
  symbol: string
  nsePrice: number
  bsePrice: number
  lastUpdated: string
  change: number
  changePercent: number
}

export type MarketOverviewResponse =
  | {
      nifty?: MarketIndex
      sensex?: MarketIndex
      banknifty?: MarketIndex
    }
  | Record<string, unknown>

export type TopMoversResponse =
  | {
      topGainers?: TrendingStock[]
      topLosers?: TrendingStock[]
    }
  | Record<string, unknown>

export type CompareStockResponseItem = {
  Ticker: string
  Details: StockDetailsResponse
}

export type WatchlistItem = { symbol: string } | string

