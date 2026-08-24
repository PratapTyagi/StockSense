import { NavLink } from "react-router-dom";
import type { IAppNavigationProp } from "../../interfaces/IAppNavigation";

export const DesktopNavigation = ({ navItems }: IAppNavigationProp) => {
  return (
    <nav
      className="hidden sm:flex items-center gap-1"
      aria-label="Main navigation"
    >
      {navItems.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.to === "/"}
          className={({ isActive }) =>
            `px-4 py-2 rounded-lg text-sm font-medium transition-colors ${
              isActive
                ? "bg-brand/15 text-brand-light"
                : "text-text-secondary hover:text-text-primary hover:bg-surface-700"
            }`
          }
        >
          <span className="mr-1.5">{item.icon}</span>
          {item.label}
        </NavLink>
      ))}
    </nav>
  );
};
