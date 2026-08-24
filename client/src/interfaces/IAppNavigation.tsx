export interface IAppNavigationProp {
  navItems: INavigationItem[];
}

export interface INavigationItem {
  to: string;
  label: string;
  icon?: string;
}
