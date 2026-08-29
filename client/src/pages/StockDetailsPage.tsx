import { useParams, Link } from "react-router-dom";
import { useState } from "react";
import {
  useOpportunities,
  useOpportunityExplanation,
  useStockDetails,
  useWatchlist,
  useWatchlistAdd,
  useWatchlistRemove,
} from "../hooks/queries";
import { ScoreBar } from "../components/ui/ScoreBar";
import { ErrorState } from "../components/ui/ErrorState";
import { StockDetailsSkeleton, Skeleton } from "../components/ui/Skeleton";

export function StockDetailsPage() {
  const { symbol } = useParams<{ symbol: string }>();
  const decodedSymbol = symbol ? decodeURIComponent(symbol) : "";

  const { data: opportunities, isLoading: oppLoading } = useOpportunities({
    top: 200,
  });
  const { data: stockDetails, isLoading: detailsLoading } =
    useStockDetails(decodedSymbol);

  const opportunity = opportunities?.find(
    (o) => o.symbol.toLowerCase() === decodedSymbol.toLowerCase(),
  );

  // Watchlist
  const { data: watchlist } = useWatchlist();
  const addToWatchlist = useWatchlistAdd();
  const removeFromWatchlist = useWatchlistRemove();

  const isInWatchlist = (watchlist || []).some((item) => {
    const sym = typeof item === "string" ? item : item.symbol;
    return sym.toLowerCase() === decodedSymbol.toLowerCase();
  });

  const [showExplanation, setShowExplanation] = useState(false);
  const {
    data: explanation,
    isLoading: explanationLoading,
    error: explanationError,
    refetch: refetchExplanation,
  } = useOpportunityExplanation(decodedSymbol, showExplanation);

  if (oppLoading || detailsLoading) {
    return <StockDetailsSkeleton />;
  }

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Back link */}
      <Link
        to="/"
        className="inline-flex items-center gap-1 text-sm text-text-secondary hover:text-text-primary transition-colors"
      >
        ← Back to Opportunities
      </Link>

      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-text-primary">
            {decodedSymbol}
          </h1>
          {stockDetails && (
            <p className="text-sm text-text-secondary">
              {stockDetails.companyName}
            </p>
          )}
        </div>
        <div className="flex items-center gap-3">
          {opportunity && (
            <div className="flex items-center gap-2">
              <span className="text-sm text-text-muted">₹</span>
              <span className="text-xl font-bold text-text-primary">
                {opportunity.latestClose.toLocaleString("en-IN", {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2,
                })}
              </span>
            </div>
          )}
          <button
            onClick={() =>
              isInWatchlist
                ? removeFromWatchlist.mutate(decodedSymbol)
                : addToWatchlist.mutate(decodedSymbol)
            }
            disabled={addToWatchlist.isPending || removeFromWatchlist.isPending}
            className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-colors cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed ${
              isInWatchlist
                ? "bg-warning/15 text-warning hover:bg-warning/25"
                : "bg-brand/15 text-brand-light hover:bg-brand/25"
            }`}
            aria-label={
              isInWatchlist ? "Remove from watchlist" : "Add to watchlist"
            }
          >
            <span>{isInWatchlist ? "⭐" : "☆"}</span>
            {isInWatchlist ? "Watching" : "Watch"}
          </button>
        </div>
      </div>

      {/* Opportunity Score Card */}
      {opportunity && (
        <div className="card p-6 space-y-5">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-text-primary">
              Opportunity Score
            </h2>
            <div className="text-3xl font-bold text-brand-light">
              {opportunity.opportunityScore.toFixed(1)}
            </div>
          </div>

          {/* Score breakdown */}
          <div className="space-y-3">
            <ScoreBar
              label="Momentum"
              value={opportunity.scores.momentumScore}
            />
            <ScoreBar label="Trend" value={opportunity.scores.trendScore} />
            <ScoreBar label="Volume" value={opportunity.scores.volumeScore} />
            <ScoreBar label="Risk" value={opportunity.scores.riskScore} />
          </div>

          {/* Signals */}
          {opportunity.signals.length > 0 && (
            <div className="border-t border-border-subtle pt-4">
              <h3 className="text-sm font-medium text-text-secondary mb-2">
                Signals
              </h3>
              <div className="flex flex-wrap gap-2">
                {opportunity.signals.map((signal, i) => (
                  <span
                    key={i}
                    className="text-xs bg-surface-700 text-text-secondary px-2.5 py-1 rounded-md"
                  >
                    {signal}
                  </span>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Key Metrics */}
      {opportunity && (
        <div className="card p-6">
          <h2 className="text-lg font-semibold text-text-primary mb-4">
            Key Metrics
          </h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4">
            <MetricItem
              label="1M Return"
              value={opportunity.metrics.oneMonthReturnPct}
              suffix="%"
              colored
            />
            <MetricItem
              label="3M Return"
              value={opportunity.metrics.threeMonthReturnPct}
              suffix="%"
              colored
            />
            <MetricItem
              label="6M Return"
              value={opportunity.metrics.sixMonthReturnPct}
              suffix="%"
              colored
            />
            <MetricItem
              label="Volume Ratio"
              value={opportunity.metrics.volumeRatio}
              suffix="x"
            />
            <MetricItem
              label="Dist. SMA50"
              value={opportunity.metrics.distanceFromSma50Pct}
              suffix="%"
              colored
            />
            <MetricItem
              label="Dist. SMA200"
              value={opportunity.metrics.distanceFromSma200Pct}
              suffix="%"
              colored
            />
            <MetricItem
              label="Volatility"
              value={opportunity.metrics.annualizedVolatilityPct}
              suffix="%"
            />
            <MetricItem label="Exchange" textValue={opportunity.exchange} />
          </div>
        </div>
      )}

      {/* Stock Info */}
      {stockDetails && (
        <div className="card p-6">
          <h2 className="text-lg font-semibold text-text-primary mb-4">
            Stock Information
          </h2>
          <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
            <MetricItem
              label="NSE Price"
              value={stockDetails.nsePrice}
              prefix="₹"
            />
            <MetricItem
              label="BSE Price"
              value={stockDetails.bsePrice}
              prefix="₹"
            />
            <MetricItem
              label="Last Updated"
              textValue={new Date(
                stockDetails.lastUpdated,
              ).toLocaleDateString()}
            />
          </div>

          {/* News */}
          {stockDetails.news && stockDetails.news.length > 0 && (
            <div className="mt-6 border-t border-border-subtle pt-4">
              <h3 className="text-sm font-medium text-text-secondary mb-3">
                Recent News
              </h3>
              <div className="space-y-3">
                {stockDetails.news.slice(0, 5).map((item, i) => (
                  <a
                    key={i}
                    href={item.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="block p-3 rounded-lg bg-surface-700/50 hover:bg-surface-700 transition-colors"
                  >
                    <p className="text-sm font-medium text-text-primary">
                      {item.headline}
                    </p>
                    <p className="text-xs text-text-muted mt-1">
                      {item.source} •{" "}
                      {new Date(item.publishedAt).toLocaleDateString()}
                    </p>
                  </a>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* AI Explanation */}
      <div className="card p-6 space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-lg font-semibold text-text-primary">
            AI Explanation
          </h2>
          {!showExplanation && !explanation && (
            <button
              onClick={() => setShowExplanation(true)}
              className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-gradient-to-r from-brand to-brand-light text-white text-sm font-medium hover:opacity-90 transition-opacity"
            >
              <span>✨</span>
              Explain with AI
            </button>
          )}
        </div>

        {showExplanation && explanationLoading && (
          <div className="space-y-3">
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-5/6" />
            <Skeleton className="h-4 w-4/6" />
            <Skeleton className="h-4 w-full" />
            <Skeleton className="h-4 w-3/4" />
          </div>
        )}

        {showExplanation && explanationError && (
          <ErrorState
            title="Explanation failed"
            message="Could not generate AI explanation. Please try again."
            onRetry={() => refetchExplanation()}
          />
        )}

        {explanation && (
          <div className="space-y-5 animate-fade-in">
            {/* Summary */}
            <div>
              <h3 className="text-sm font-semibold text-brand-light flex items-center gap-1.5 mb-2">
                <span>✨</span> Why it stands out
              </h3>
              <p className="text-sm text-text-secondary leading-relaxed">
                {explanation.summary}
              </p>
            </div>

            {/* Strengths */}
            {explanation.strengths.length > 0 && (
              <div>
                <h3 className="text-sm font-semibold text-positive flex items-center gap-1.5 mb-2">
                  <span>✓</span> Strengths
                </h3>
                <ul className="space-y-1.5">
                  {explanation.strengths.map((s, i) => (
                    <li
                      key={i}
                      className="text-sm text-text-secondary flex items-start gap-2"
                    >
                      <span className="text-positive mt-0.5">•</span>
                      {s}
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Watch points */}
            {explanation.watchPoints.length > 0 && (
              <div>
                <h3 className="text-sm font-semibold text-warning flex items-center gap-1.5 mb-2">
                  <span>⚠</span> Watch points
                </h3>
                <ul className="space-y-1.5">
                  {explanation.watchPoints.map((w, i) => (
                    <li
                      key={i}
                      className="text-sm text-text-secondary flex items-start gap-2"
                    >
                      <span className="text-warning mt-0.5">•</span>
                      {w}
                    </li>
                  ))}
                </ul>
              </div>
            )}

            {/* Overall */}
            <div className="border-t border-border-subtle pt-4">
              <h3 className="text-sm font-semibold text-text-primary mb-2">
                Overall
              </h3>
              <p className="text-sm text-text-secondary leading-relaxed">
                {explanation.overall}
              </p>
            </div>
          </div>
        )}

        {!showExplanation && !explanation && (
          <p className="text-sm text-text-muted">
            Click "Explain with AI" to get an AI-powered interpretation of this
            stock's opportunity score.
          </p>
        )}
      </div>
    </div>
  );
}

// ─── Helper component ─────────────────────────────────────────────────────────

type MetricItemProps = {
  label: string;
  value?: number | null;
  textValue?: string;
  prefix?: string;
  suffix?: string;
  colored?: boolean;
};

function MetricItem({
  label,
  value,
  textValue,
  prefix = "",
  suffix = "",
  colored = false,
}: MetricItemProps) {
  let displayValue: string;
  let colorClass = "text-text-primary";

  if (textValue !== undefined) {
    displayValue = textValue;
  } else if (value === null || value === undefined) {
    displayValue = "—";
    colorClass = "text-text-muted";
  } else {
    displayValue = `${prefix}${value.toFixed(2)}${suffix}`;
    if (colored) {
      colorClass =
        value > 0
          ? "text-positive-light"
          : value < 0
            ? "text-negative-light"
            : "text-text-primary";
    }
  }

  return (
    <div>
      <p className="text-xs text-text-muted">{label}</p>
      <p className={`text-sm font-semibold mt-0.5 ${colorClass}`}>
        {displayValue}
      </p>
    </div>
  );
}
