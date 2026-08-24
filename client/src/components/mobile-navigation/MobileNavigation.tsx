import { NavLink } from "react-router-dom";
import type { IAppNavigationProp } from "../../interfaces/IAppNavigation";

export const MobileNavigation = ({ navItems }: IAppNavigationProp) => {
  return (
    <nav
      className="sm:hidden glass sticky bottom-0 z-50 flex justify-around py-2 px-4"
      aria-label="Mobile navigation"
    >
      {navItems.map((item) => (
        <NavLink
          key={item.to}
          to={item.to}
          end={item.to === "/"}
          className={({ isActive }) =>
            `flex flex-col items-center gap-0.5 px-3 py-1.5 rounded-lg text-xs font-medium transition-colors ${
              isActive ? "text-brand-light" : "text-text-muted"
            }`
          }
        >
          <span className="text-lg">{item.icon}</span>
          <span>{item.label}</span>
        </NavLink>
      ))}
    </nav>
  );
};
