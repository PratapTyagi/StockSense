import type { OpporunitySortField } from "../../types/OpporunitySortField";

const sortOptions: { value: OpporunitySortField; label: string }[] = [
  { value: "score", label: "Overall Score" },
  { value: "momentum", label: "Momentum" },
  { value: "trend", label: "Trend" },
  { value: "volume", label: "Volume" },
  { value: "risk", label: "Low Risk" },
];

export const Filter = ({ sortBy, setSortBy, minScore, setMinScore }) => {
  return (
    <div className="flex flex-col sm:flex-row gap-3 sm:items-center">
      {/* Sort */}
      <div className="flex items-center gap-2">
        <label htmlFor="sort-select" className="text-xs text-text-muted">
          Sort by
        </label>
        <select
          id="sort-select"
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value as OpporunitySortField)}
          className="bg-surface-700 border border-border-subtle text-text-primary text-sm rounded-lg px-3 py-1.5 focus:ring-brand focus:border-brand"
        >
          {sortOptions.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
      </div>

      {/* Min score filter */}
      <div className="flex items-center gap-2">
        <label htmlFor="min-score" className="text-xs text-text-muted">
          Min score
        </label>
        <input
          id="min-score"
          type="range"
          min={0}
          max={90}
          step={5}
          value={minScore}
          onChange={(e) => setMinScore(Number(e.target.value))}
          className="w-24 accent-brand"
        />
        <span className="text-xs text-text-secondary w-6">{minScore}</span>
      </div>
    </div>
  );
};
