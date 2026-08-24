import { MarketOverviewHeader } from "../components/market-overview/MarketOverviewHeader";
import { MarketIndices } from "../components/market-indices/MarketIndices";
import { TopGainersAndLosers } from "../components/top-movers/TopGainersAndLosers";

export function MarketPage() {
  return (
    <div className="space-y-6">
      {/* Page header */}
      <MarketOverviewHeader />

      {/* Market Indices */}
      <section className="space-y-3">
        <MarketIndices />
      </section>

      {/* Top Gainers & Losers */}
      <section className="space-y-3">
        <TopGainersAndLosers />
      </section>
    </div>
  );
}
