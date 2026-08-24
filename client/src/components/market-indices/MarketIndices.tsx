import { useMarketOverview } from "../../hooks/queries";
import type { MarketIndex } from "../../types/api";
import { ErrorState } from "../ui/ErrorState";
import { Skeleton } from "../ui/Skeleton";
import { IndexCard } from "./IndexCard";

export const MarketIndices = () => {
  const {
    data: market,
    isLoading: marketLoading,
    error: marketError,
    refetch: refetchMarket,
  } = useMarketOverview();
  return (
    <>
      <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wider">
        Indices
      </h3>

      {marketLoading && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="card p-5 space-y-3">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-6 w-28" />
              <Skeleton className="h-3 w-16" />
            </div>
          ))}
        </div>
      )}

      {marketError && (
        <ErrorState
          title="Failed to load market data"
          message="Could not fetch market indices."
          onRetry={() => refetchMarket()}
        />
      )}

      {!marketLoading && !marketError && market && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 stagger-children">
          {(["nifty", "sensex", "banknifty"] as const).map((key) => {
            const index = (market as Record<string, MarketIndex | undefined>)[
              key
            ];
            if (!index) return null;
            return <IndexCard key={key} index={index} />;
          })}
        </div>
      )}
    </>
  );
};
