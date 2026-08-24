import { Link } from 'react-router-dom'
import type { OpportunityResult } from '../../types/api'
import { ScoreBar } from '../ui/ScoreBar'

type OpportunityCardProps = {
  opportunity: OpportunityResult
}

function getSignalIcon(signal: string): string {
  const lower = signal.toLowerCase()
  if (lower.includes('momentum')) return '⚡'
  if (lower.includes('trend')) return '📈'
  if (lower.includes('volume')) return '📊'
  if (lower.includes('risk') || lower.includes('volatil')) return '🛡️'
  if (lower.includes('golden')) return '✨'
  if (lower.includes('breakout')) return '🚀'
  return '•'
}

function getScoreGradient(score: number): string {
  if (score >= 80) return 'from-positive/20 to-transparent'
  if (score >= 60) return 'from-brand/20 to-transparent'
  return 'from-warning/20 to-transparent'
}

export function OpportunityCard({ opportunity }: OpportunityCardProps) {
  const { symbol, opportunityScore, scores, signals, rank, latestClose } = opportunity

  return (
    <Link
      to={`/stock/${encodeURIComponent(symbol)}`}
      className="card card-hover block p-5 relative overflow-hidden"
    >
      {/* Subtle gradient accent */}
      <div
        className={`absolute inset-0 bg-gradient-to-br ${getScoreGradient(opportunityScore)} pointer-events-none`}
      />

      <div className="relative space-y-4">
        {/* Header */}
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-2">
              <span className="text-xs font-medium text-text-muted bg-surface-700 px-2 py-0.5 rounded">
                #{rank}
              </span>
              <h3 className="text-base font-bold text-text-primary">{symbol}</h3>
            </div>
            <p className="text-xs text-text-secondary mt-0.5">
              ₹{latestClose.toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </p>
          </div>
          <div className="text-right">
            <div className="text-2xl font-bold text-text-primary">
              {opportunityScore.toFixed(1)}
            </div>
            <div className="text-[10px] text-text-muted uppercase tracking-wider">Score</div>
          </div>
        </div>

        {/* Score breakdown */}
        <div className="space-y-2">
          <ScoreBar label="Momentum" value={scores.momentumScore} />
          <ScoreBar label="Trend" value={scores.trendScore} />
          <ScoreBar label="Volume" value={scores.volumeScore} />
          <ScoreBar label="Risk" value={scores.riskScore} />
        </div>

        {/* Signals */}
        {signals.length > 0 && (
          <div className="flex flex-wrap gap-1.5">
            {signals.slice(0, 3).map((signal, i) => (
              <span
                key={i}
                className="inline-flex items-center gap-1 text-[11px] text-text-secondary bg-surface-700/60 px-2 py-1 rounded-md"
              >
                <span>{getSignalIcon(signal)}</span>
                <span className="truncate max-w-[140px]">{signal}</span>
              </span>
            ))}
            {signals.length > 3 && (
              <span className="text-[11px] text-text-muted px-2 py-1">
                +{signals.length - 3} more
              </span>
            )}
          </div>
        )}

        {/* CTA */}
        <div className="flex justify-end">
          <span className="text-xs font-medium text-brand-light group-hover:text-brand">
            View Details →
          </span>
        </div>
      </div>
    </Link>
  )
}