type ScoreBarProps = {
  label: string
  value: number
  maxValue?: number
  colorClass?: string
}

function getScoreColor(value: number): string {
  if (value >= 80) return 'bg-positive'
  if (value >= 60) return 'bg-brand-light'
  if (value >= 40) return 'bg-warning'
  return 'bg-negative'
}

export function ScoreBar({ label, value, maxValue = 100, colorClass }: ScoreBarProps) {
  const percentage = Math.min((value / maxValue) * 100, 100)
  const color = colorClass || getScoreColor(value)

  return (
    <div className="flex items-center gap-3">
      <span className="text-xs text-text-secondary w-20 shrink-0">{label}</span>
      <div className="flex-1 h-2 bg-surface-700 rounded-full overflow-hidden">
        <div
          className={`h-full rounded-full score-bar-fill ${color}`}
          style={{ '--score-width': `${percentage}%` } as React.CSSProperties}
        />
      </div>
      <span className="text-xs font-semibold text-text-primary w-8 text-right">
        {Math.round(value)}
      </span>
    </div>
  )
}