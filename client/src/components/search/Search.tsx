import Table, { type Column } from "../../ui/table/Table";
import SearchBar from "../searchBar/SearchBar";
import "./Search.css";

const SearchResults = ({ searchTerm }: { searchTerm: string }) => {
  const data = [
    {
      title: "Apple Inc.",
      description: "Technology company",
      price: 150.0,
      change: 2.5,
    },
    {
      title: "Microsoft Corporation",
      description: "Software company",
      price: 300.0,
      change: -1.2,
    },
    {
      title: "Amazon.com Inc.",
      description: "E-commerce company",
      price: 3500.0,
      change: 5.8,
    },
    {
      title: "Google LLC",
      description: "Internet company",
      price: 2800.0,
      change: -3.1,
    },
    {
      title: "Tesla Inc.",
      description: "Electric vehicle manufacturer",
      price: 800.0,
      change: 10.2,
    },
    {
        title: "Meta Platforms Inc.",
        description: "Social media company",
        price: 250.0,
        change: -2.3,
    },
    
    {
      title: "Google LLC",
      description: "Internet company",
      price: 2800.0,
      change: -3.1,
    },
    {
      title: "Tesla Inc.",
      description: "Electric vehicle manufacturer",
      price: 800.0,
      change: 10.2,
    },
    {
        title: "Meta Platforms Inc.",
        description: "Social media company",
        price: 250.0,
        change: -2.3,
    },
    {
      title: "Google LLC",
      description: "Internet company",
      price: 2800.0,
      change: -3.1,
    },
    {
      title: "Tesla Inc.",
      description: "Electric vehicle manufacturer",
      price: 800.0,
      change: 10.2,
    },
    {
        title: "Meta Platforms Inc.",
        description: "Social media company",
        price: 250.0,
        change: -2.3,
    },
    {
        title: "Meta Platforms Inc.",
        description: "Social media company",
        price: 250.0,
        change: -2.3,
    },
    {
      title: "Google LLC",
      description: "Internet company",
      price: 2800.0,
      change: -3.1,
    },
    {
      title: "Tesla Inc.",
      description: "Electric vehicle manufacturer",
      price: 800.0,
      change: 10.2,
    },
    {
        title: "Meta Platforms Inc.",
        description: "Social media company",
        price: 250.0,
        change: -2.3,
    },

  ];
  const columns: Column<any>[] = [
    {
      header: 'Name',
      accessor: 'title',
      className: 'font-medium nowrap',
    },
    {
      header: 'Description',
      accessor: 'description',
    },
    {
      header: 'Price',
      accessor: 'price',
      align: 'left',
      render: (_: any, value: number) => `$${value.toFixed(2)}`,
    },
    {
      header: 'Change',
      accessor: 'change',
      align: 'left',
      render: (_: any, value: number) => (
        <span className={value >= 0 ? 'color-green' : 'color-red'}>
          {value >= 0 ? `+${value.toFixed(2)}%` : `${value.toFixed(2)}%`}
        </span>
      ),
    },
  ];

  return (
    <>
      <div className="header flex justify-between w-full pb-5">
        <p className="primary-heading">TOP RESULTS FOR "{searchTerm}"</p>
        <span className="secondary-heading">Showing 1-5 of 42 results</span>
      </div>
      <Table data={data} columns={columns} />
    </>
  );
};

const RecentSearches = () => {
  return (
    <div className="recent-searches p-5 shadow-sm">
      <div className="header flex justify-between w-full mb-4">
        <p className="primary-heading">RECENT SEARCHES</p>
        <button className="button secondary-heading text-xs">Clear</button>
      </div>
      <ul className="search-list">
        <li className="search-item">AAPL</li>
        <li className="search-item">MSFT</li>
        <li className="search-item">AMZN</li>
      </ul>
    </div>
  );
};

const TrendingNow = () => {
  const trendingStocks = [
    { symbol: "AAPL", name: "Apple Inc.", price: 150.0, change: 2.5 },
    {
      symbol: "MSFT",
      name: "Microsoft Corporation",
      price: 300.0,
      change: -1.2,
    },
    { symbol: "AMZN", name: "Amazon.com Inc.", price: 3500.0, change: 5.8 },
    { symbol: "GOOGL", name: "Google LLC", price: 2800.0, change: -3.1 },
    { symbol: "TSLA", name: "Tesla Inc.", price: 800.0, change: 10.2 },
  ];
  return (
    <div className="trending-now p-5 shadow-sm">
      <p className="primary-heading mb-4">TRENDING NOW</p>
      <ul className="search-list">
        {trendingStocks.map((stock, index) => (
          <li key={stock.symbol} className="search-item flex items-center p-2">
            <span>{index + 1}</span>
            <div className="pl-4 flex flex-col">
              <span className="primary-heading symbol">{stock.symbol}</span>
              <span className="secondary-heading">{stock.name}</span>
            </div>
            <div className="flex flex-col items-end ml-auto">
              <span
                className={`change ${stock.change >= 0 ? "color-green" : "color-red"}`}
              >
                {stock.change >= 0
                  ? `+${stock.change.toFixed(2)}%`
                  : `${stock.change.toFixed(2)}%`}
              </span>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};

const Search = () => {
  const searchTerm = "AAPL";
  return (
    <div className="search-container">
      <section
        className="dashboard-navbar"
        style={{
          width: "85vw",
          background: "white",
          padding: "20px",
          borderLeft: "1px solid grey",
        }}
      >
        Navbar
      </section>
      <SearchBar />
      <div className="search-details">
        <div className="search-results-container">
          <SearchResults searchTerm={searchTerm} />
        </div>
        <div className="top-searches">
          <RecentSearches />
          <TrendingNow />
        </div>
      </div>
    </div>
  );
};

export default Search;
