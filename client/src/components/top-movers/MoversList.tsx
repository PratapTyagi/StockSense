import { Link } from "react-router-dom";
import type { TrendingStock } from "../../types/api";

export const MoversList = ({
  title,
  stocks,
  type,
}: {
  title: string;
  stocks: TrendingStock[];
  type: "gainer" | "loser";
}) => {
  console.log({ stocks });

  return (
    <div className="card p-5">
      <h4 className="text-sm font-semibold text-text-primary mb-3">{title}</h4>
      <div className="space-y-2">
        {stocks.slice(0, 8).map((stock) => (
          <Link
            key={stock.symbol}
            to={`/stock/${encodeURIComponent(stock.companyName)}`}
            className="flex items-center justify-between p-2 rounded-lg hover:bg-surface-700 transition-colors"
          >
            <span className="text-sm font-medium text-text-primary">
              {stock.companyName}
            </span>
            <div className="text-right">
              <span className="text-xs text-text-secondary mr-3">
                ₹
                {stock.price?.toLocaleString("en-IN", {
                  maximumFractionDigits: 2,
                }) ?? "—"}
              </span>
              <span
                className={`text-xs font-semibold ${
                  type === "gainer" ? "text-positive" : "text-negative"
                }`}
              >
                {stock.changePercent >= 0 ? "+" : ""}
                {stock.changePercent?.toFixed(2) ?? "0.00"}%
              </span>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
};
