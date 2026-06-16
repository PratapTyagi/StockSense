import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from '../services/api'

export const queryKeys = {
  market: ['marketOverview'] as const,
  trending: ['trending'] as const,
  topMovers: ['topMovers'] as const,
  stockDetails: (symbol: string) => ['stockDetails', symbol] as const,
  stockHistory: (symbol: string, range: string) =>
    ['stockHistory', symbol, range] as const,
  compare: (symbols: string[]) => ['compare', symbols.join(',')] as const,
  watchlist: ['watchlist'] as const,
  batch: (symbols: string[]) => ['batch', symbols.join(',')] as const,
}

export function useMarketOverview() {
  return useQuery({ queryKey: queryKeys.market, queryFn: api.marketOverview })
}

export function useTrendingStocks() {
  return useQuery({ queryKey: queryKeys.trending, queryFn: api.trending })
}

export function useTopMovers() {
  return useQuery({ queryKey: queryKeys.topMovers, queryFn: api.topMovers })
}

export function useStockDetails(symbol: string) {
  return useQuery({
    queryKey: queryKeys.stockDetails(symbol),
    queryFn: () => api.stockDetails(symbol),
    enabled: !!symbol,
  })
}

export function useStockHistory(symbol: string, range: string) {
  return useQuery({
    queryKey: queryKeys.stockHistory(symbol, range),
    queryFn: () => api.stockHistory(symbol, range),
    enabled: !!symbol && !!range,
  })
}

export function useCompare(symbols: string[]) {
  return useQuery({
    queryKey: queryKeys.compare(symbols),
    queryFn: () => api.compare(symbols),
    enabled: symbols.length > 0,
  })
}

export function useWatchlist() {
  return useQuery({ queryKey: queryKeys.watchlist, queryFn: api.watchlistGet })
}

export function useBatch(symbols: string[]) {
  return useQuery({
    queryKey: queryKeys.batch(symbols),
    queryFn: () => api.batch(symbols),
    enabled: symbols.length > 0,
  })
}

export function useWatchlistMutations() {
  const qc = useQueryClient()

  const add = useMutation({
    mutationFn: (symbol: string) => api.watchlistAdd(symbol),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.watchlist }),
  })

  const remove = useMutation({
    mutationFn: (symbol: string) => api.watchlistRemove(symbol),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.watchlist }),
  })

  return { add, remove }
}

