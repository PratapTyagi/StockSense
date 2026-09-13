import { useState, useMemo } from "react";
import { useOpportunities } from "../hooks/queries";
import { OpportunityCard } from "../components/opportunities/OpportunityCard";
import { OpportunityCardSkeleton } from "../components/ui/Skeleton";
import { ErrorState } from "../components/ui/ErrorState";
import { EmptyState } from "../components/ui/EmptyState";
import { MarketOpportunityHeader } from "../components/market-opportunities/MarketOpportunityHeader";
import { Filter } from "../components/market-opportunities/Filter";
import type { OpporunitySortField } from "../types/OpporunitySortField";

export function OpportunitiesPage() {
  const [sortBy, setSortBy] = useState<OpporunitySortField>("score");
  const [minScore, setMinScore] = useState<number>(0);

  const {
    data: opportunities,
    isLoading,
    error,
    refetch,
  } = useOpportunities({ top: 50 });

  const sorted = useMemo(() => {
    if (!opportunities) return [];

    let filtered = opportunities.filter((o) => o.opportunityScore >= minScore);

    switch (sortBy) {
      case "momentum":
        return [...filtered].sort(
          (a, b) => b.scores.momentumScore - a.scores.momentumScore,
        );
      case "trend":
        return [...filtered].sort(
          (a, b) => b.scores.trendScore - a.scores.trendScore,
        );
      case "volume":
        return [...filtered].sort(
          (a, b) => b.scores.volumeScore - a.scores.volumeScore,
        );
      case "risk":
        return [...filtered].sort(
          (a, b) => b.scores.riskScore - a.scores.riskScore,
        );
      default:
        return [...filtered].sort(
          (a, b) => b.opportunityScore - a.opportunityScore,
        );
    }
  }, [opportunities, sortBy, minScore]);

  return (
    <div className="space-y-6">
      {/* Page header */}
      <MarketOpportunityHeader />

      {/* Filters */}
      <Filter
        sortBy={sortBy}
        setSortBy={(value) => {
          setSortBy(value);
        }}
        minScore={minScore}
        setMinScore={(value) => {
          setMinScore(value);
        }}
      />

      {/* Content */}
      {isLoading && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 stagger-children">
          {Array.from({ length: 6 }).map((_, i) => (
            <OpportunityCardSkeleton key={i} />
          ))}
        </div>
      )}

      {error && (
        <ErrorState
          title="Failed to load opportunities"
          message="The opportunity scanner encountered an error. Please try again."
          onRetry={() => refetch()}
        />
      )}

      {!isLoading && !error && sorted.length === 0 && (
        <EmptyState
          icon="🔍"
          title="No opportunities found"
          description={
            minScore > 0
              ? `No stocks match the minimum score of ${minScore}. Try lowering the threshold.`
              : "No opportunity data available at the moment."
          }
        />
      )}

      {!isLoading && !error && sorted.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4 stagger-children">
          {sorted.map((opportunity) => (
            <OpportunityCard
              key={opportunity.symbol}
              opportunity={opportunity}
            />
          ))}
        </div>
      )}
    </div>
  );
}
