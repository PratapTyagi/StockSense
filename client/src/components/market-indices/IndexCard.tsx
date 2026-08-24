import type { MarketIndex } from "../../types/api";

export const IndexCard = ({ index }: { index: MarketIndex }) => {
  const isPositive = (index.changePercent ?? 0) >= 0;

  return (
    <div className="card p-5">
      <p className="text-xs text-text-muted uppercase tracking-wider">
        {index.companyName || index.symbol}
      </p>
      <p className="text-xl font-bold text-text-primary mt-1">
        {(index.nsePrice || index.bsePrice || 0).toLocaleString("en-IN", {
          maximumFractionDigits: 2,
        })}
      </p>
      <p
        className={`text-sm font-medium mt-1 ${isPositive ? "text-positive" : "text-negative"}`}
      >
        {isPositive ? "+" : ""}
        {(index.changePercent ?? 0).toFixed(2)}%
      </p>
    </div>
  );
};
