import { useTrending } from "../../hooks/queries";
import { ErrorState } from "../ui/ErrorState";
import { Skeleton } from "../ui/Skeleton";
import { MoversList } from "./MoversList";

export const TopGainersAndLosers = () => {
  const {
    data: trending,
    isLoading: trendingLoading,
    error: trendingError,
    refetch: refetchTrending,
  } = useTrending();

  return (
    <>
      <h3 className="text-sm font-semibold text-text-secondary uppercase tracking-wider">
        Top Movers
      </h3>

      {trendingLoading && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {Array.from({ length: 2 }).map((_, i) => (
            <div key={i} className="card p-5 space-y-3">
              <Skeleton className="h-4 w-24" />
              {Array.from({ length: 5 }).map((_, j) => (
                <Skeleton key={j} className="h-8 w-full" />
              ))}
            </div>
          ))}
        </div>
      )}

      {trendingError && (
        <ErrorState
          title="Failed to load trending stocks"
          message="Could not fetch top movers."
          onRetry={() => refetchTrending()}
        />
      )}

      {!trendingLoading && !trendingError && trending && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {trending.topGainers && trending.topGainers.length > 0 && (
            <MoversList
              title="🟢 Top Gainers"
              stocks={trending.topGainers}
              type="gainer"
            />
          )}
          {trending.topLosers && trending.topLosers.length > 0 && (
            <MoversList
              title="🔴 Top Losers"
              stocks={trending.topLosers}
              type="loser"
            />
          )}
        </div>
      )}
    </>
  );
};
