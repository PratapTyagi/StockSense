export const Logo = () => {
  return (
    <div className="flex items-center gap-2">
      <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-brand to-brand-light flex items-center justify-center">
        <span className="text-white text-sm font-bold">S</span>
      </div>
      <h1 className="text-lg font-bold text-text-primary">
        StockSense<span className="text-brand-light">AI</span>
      </h1>
    </div>
  );
};
