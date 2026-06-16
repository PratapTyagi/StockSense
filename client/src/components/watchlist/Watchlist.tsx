import Table, { type Column } from "../../ui/table/Table";
import "./Watchlist.css";

const Watchlist = () => {
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


    const myLists = [
        {
            name: "My Favorites",
            isActive: true,
            stocks: [
                { symbol: "AAPL", name: "Apple Inc.", price: 150.0, change: 2.5 },
                { symbol: "MSFT", name: "Microsoft Corporation", price: 300.0, change: -1.2 },
                { symbol: "AMZN", name: "Amazon.com Inc.", price: 3500.0, change: 5.8 },
            ],
        },
        {
            name: "Long term",
            stocks: [
                { symbol: "GOOGL", name: "Google LLC", price: 2800.0, change: -3.1 },
                { symbol: "TSLA", name: "Tesla Inc.", price: 800.0, change: 10.2 },
            ],
        },
        {
            name: "Daily watch",
            stocks: [
                { symbol: "GOOGL", name: "Google LLC", price: 2800.0, change: -3.1 },
                { symbol: "TSLA", name: "Tesla Inc.", price: 800.0, change: 10.2 },
                { symbol: "META", name: "Meta Platforms Inc.", price: 250.0, change: -2.3 },
                { symbol: "NFLX", name: "Netflix Inc.", price: 500.0, change: 4.5 },
                { symbol: "NVDA", name: "NVIDIA Corporation", price: 600.0, change: 6.7 },
                { symbol: "DIS", name: "The Walt Disney Company", price: 180.0, change: -1.8 },
            ],
        }
    ];
    
    const results = [
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

    const activeList = myLists.find(list => list.isActive);
    const selectedListCategory = activeList ? activeList.name : "No List Selected";

    return (
        <div className="watchlist-container">
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

            <div className="watchlist-content p-10 flex gap-10">
                <div className="my-lists">
                    <p className="container-heading mb-2">My Lists</p>
                    <ul className="list-categories">
                        {myLists.map((list) => (
                            <li key={list.name} className={`list-category flex justify-between w-full p-2 ${list.isActive ? "active" : ""}`.trim()}>
                                <div className="flex items-center">
                                    <img className="category-icon" src="/watch-list-item.png" alt="" />
                                    <p className={`pl-2 ${list.isActive ? "primary-heading" : "secondary-heading"}`}>{list.name}</p>
                                </div>
                                <span className="total-stocks">{list.stocks.length}</span>
                            </li>
                        ))}
                    </ul>
                    <hr />
                    <button className="button create-list" onClick={() => {}}>Create New List</button>
                </div>
                <div className="selected-list">
                    <div className="header flex items-center gap-4 pt-8 pb-8 pl-4">
                        <p className="container-heading">{selectedListCategory}</p>
                        <span className="total-stocks">{activeList ? activeList.stocks.length : 0} symbols</span>
                    </div>
                    <Table data={results} columns={columns} />
                </div>
            </div>
        </div>
    )
}

export default Watchlist