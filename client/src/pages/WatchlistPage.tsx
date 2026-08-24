import { Link } from "react-router-dom";
import {
  useWatchlist,
  useWatchlistRemove,
  useOpportunities,
} from "../hooks/queries";
import { ErrorState } from "../components/ui/ErrorState";
import { EmptyState } from "../components/ui/EmptyState";
import { Skeleton } from "../components/ui/Skeleton";
import { WatchListHeader } from "../components/watchList/WatchListHeader";

export function WatchlistPage() {
  const { data: watchlist, isLoading, error, refetch } = useWatchlist();
  const { data: opportunities } = useOpportunities({ top: 200 });
  const removeMutation = useWatchlistRemove();

  const watchlistSymbols = (watchlist || []).map((item) =>
    typeof item === "string" ? item : item.symbol,
  );

  const getOpportunity = (symbol: string) =>
    opportunities?.find((o) => o.symbol.toLowerCase() === symbol.toLowerCase());

  return (
    <div className="space-y-6">
      {/* Page header */}
      <WatchListHeader />

      {/* Loading */}
      {isLoading && (
        <div className="space-y-3">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="card p-4 flex items-center gap-4">
              <Skeleton className="h-10 w-10 rounded-lg" />
              <div className="flex-1 space-y-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-3 w-32" />
              </div>
              <Skeleton className="h-8 w-16" />
            </div>
          ))}
        </div>
      )}

      {/* Error */}
      {error && (
        <ErrorState
          title="Failed to load watchlist"
          message="Could not fetch your watchlist. Please try again."
          onRetry={() => refetch()}
        />
      )}

      {/* Empty */}
      {!isLoading && !error && watchlistSymbols.length === 0 && (
        <EmptyState
          icon="⭐"
          title="Your watchlist is empty"
          description="Add stocks from the Opportunities page to track them here."
          action={
            <Link
              to="/"
              className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-brand text-white text-sm font-medium hover:bg-brand-dark transition-colors"
            >
              Browse Opportunities
            </Link>
          }
        />
      )}

      {/* Watchlist items */}
      {!isLoading && !error && watchlistSymbols.length > 0 && (
        <div className="space-y-3 stagger-children">
          {watchlistSymbols.map((symbol) => {
            const opp = getOpportunity(symbol);
            return (
              <div
                key={symbol}
                className="card card-hover p-4 flex items-center gap-4"
              >
                {/* Score badge */}
                <div className="w-12 h-12 rounded-xl bg-surface-700 flex items-center justify-center shrink-0">
                  {opp ? (
                    <span className="text-sm font-bold text-brand-light">
                      {opp.opportunityScore.toFixed(0)}
                    </span>
                  ) : (
                    <span className="text-sm text-text-muted">—</span>
                  )}
                </div>

                {/* Info */}
                <div className="flex-1 min-w-0">
                  <Link
                    to={`/stock/${encodeURIComponent(symbol)}`}
                    className="text-sm font-bold text-text-primary hover:text-brand-light transition-colors"
                  >
                    {symbol}
                  </Link>
                  {opp && (
                    <p className="text-xs text-text-secondary mt-0.5">
                      ₹
                      {opp.latestClose.toLocaleString("en-IN", {
                        minimumFractionDigits: 2,
                      })}
                      {opp.signals.length > 0 && (
                        <span className="ml-2 text-text-muted">
                          • {opp.signals[0]}
                        </span>
                      )}
                    </p>
                  )}
                </div>

                {/* Actions */}
                <div className="flex items-center gap-2 shrink-0">
                  <Link
                    to={`/stock/${encodeURIComponent(symbol)}`}
                    className="text-xs text-brand-light hover:text-brand font-medium px-3 py-1.5 rounded-lg bg-brand/10 transition-colors"
                  >
                    Details
                  </Link>
                  <button
                    onClick={() => removeMutation.mutate(symbol)}
                    disabled={removeMutation.isPending}
                    className="text-xs text-negative hover:text-negative-light font-medium px-3 py-1.5 rounded-lg bg-negative/10 transition-colors disabled:opacity-50"
                    aria-label={`Remove ${symbol} from watchlist`}
                  >
                    Remove
                  </button>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
