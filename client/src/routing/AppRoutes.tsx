import { BrowserRouter, Route, Routes } from 'react-router-dom'
import { AppLayout } from '../components/layout/AppLayout'
import { OpportunitiesPage } from '../pages/OpportunitiesPage'
import { StockDetailsPage } from '../pages/StockDetailsPage'
import { WatchlistPage } from '../pages/WatchlistPage'
import { MarketPage } from '../pages/MarketPage'

export function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<AppLayout />}>
          <Route path="/" element={<OpportunitiesPage />} />
          <Route path="/stock/:symbol" element={<StockDetailsPage />} />
          <Route path="/market" element={<MarketPage />} />
          <Route path="/watchlist" element={<WatchlistPage />} />
        </Route>
      </Routes>
    </BrowserRouter>
  )
}