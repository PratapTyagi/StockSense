import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "../services/api";

// ─── Opportunities ────────────────────────────────────────────────────────────

export function useOpportunities(params?: {
  top?: number;
  minScore?: number;
  exchange?: string;
}) {
  return useQuery({
    queryKey: ["opportunities", params],
    queryFn: () => api.opportunities(params),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

export function useOpportunityExplanation(symbol: string, enabled = true) {
  return useQuery({
    queryKey: ["opportunity-explanation", symbol],
    queryFn: () => api.opportunityExplanation(symbol),
    enabled: !!symbol && enabled,
    staleTime: 60 * 60 * 1000, // 10 minutes
  });
}

// ─── Stock Details ────────────────────────────────────────────────────────────

export function useStockDetails(symbol: string) {
  return useQuery({
    queryKey: ["stock-details", symbol],
    queryFn: () => api.stockDetails(symbol),
    enabled: !!symbol,
    staleTime: 2 * 60 * 1000,
  });
}

export function useStockHistory(symbol: string, range: string) {
  return useQuery({
    queryKey: ["stock-history", symbol, range],
    queryFn: () => api.stockHistory(symbol, range),
    enabled: !!symbol,
    staleTime: 5 * 60 * 1000,
  });
}

// ─── Market ───────────────────────────────────────────────────────────────────

export function useMarketOverview() {
  return useQuery({
    queryKey: ["market-overview"],
    queryFn: () => api.marketOverview(),
    staleTime: 2 * 60 * 1000,
  });
}

export function useTopMovers() {
  return useQuery({
    queryKey: ["top-movers"],
    queryFn: () => api.topMovers(),
    staleTime: 2 * 60 * 1000,
  });
}

export function useTrending() {
  return useQuery({
    queryKey: ["trending"],
    queryFn: () => api.trending(),
    staleTime: 2 * 60 * 1000,
  });
}

// ─── Watchlist ────────────────────────────────────────────────────────────────

export function useWatchlist() {
  return useQuery({
    queryKey: ["watchlist"],
    queryFn: () => api.watchlistGet(),
    staleTime: 30 * 1000,
  });
}

export function useWatchlistAdd() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (symbol: string) => api.watchlistAdd(symbol),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["watchlist"] });
    },
  });
}

export function useWatchlistRemove() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (symbol: string) => api.watchlistRemove(symbol),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["watchlist"] });
    },
  });
}
