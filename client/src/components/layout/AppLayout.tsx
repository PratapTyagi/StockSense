import { Outlet } from "react-router-dom";
import { Logo } from "../logo/Logo";
import { DesktopNavigation } from "../desktop-navigation/DesktopNavigation";
import type { INavigationItem } from "../../interfaces/IAppNavigation";
import { MobileNavigation } from "../mobile-navigation/MobileNavigation";

const navItems: INavigationItem[] = [
  { to: "/", label: "Opportunities", icon: "🔥" },
  { to: "/market", label: "Market", icon: "📊" },
  { to: "/watchlist", label: "Watchlist", icon: "⭐" },
];

export function AppLayout() {
  return (
    <div className="min-h-screen flex flex-col">
      {/* Header */}
      <header className="glass sticky top-0 z-50 px-4 py-3 sm:px-6">
        <div className="max-w-7xl mx-auto flex items-center justify-between">
          <Logo />
          {/* Desktop nav */}
          <DesktopNavigation navItems={navItems} />
        </div>
      </header>

      {/* Main content */}
      <main className="flex-1 px-4 py-6 sm:px-6">
        <div className="max-w-7xl mx-auto">
          <Outlet />
        </div>
      </main>

      {/* Mobile bottom nav */}
      <MobileNavigation navItems={navItems} />
    </div>
  );
}
