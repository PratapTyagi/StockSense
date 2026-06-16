import { Button } from "../../ui";
import './QuickActions.css';

const TopMover = ({ mover }: { mover: { name: string; image: string; amount: number; change: number } }) => {
    return (
        <div className="top-mover gap-3 mt-2">
            <img src={mover.image} alt={mover.name} className="logo" />
            <div className="top-mover-details">
                <p className="mover-name secondary-heading">{mover.name}</p>
                <section className="top-mover-stats flex flex-col align-items-center">
                    <p className="top-mover-amount">₹ {mover.amount.toFixed(2)}</p>
                    <span className={`top-mover-change ${mover.change >= 0 ? 'color-green' : 'color-red'}`}>
                        {mover.change >= 0 ? `+${mover.change}%` : `${mover.change}%`}
                    </span>
                </section>
            </div>
        </div>
    );
}

const QuickActions = () => {
    const topMovers = [
        { name: "AAPL", image: "/temp-profile-pic.png", amount: 150.25, change: 2.5 },
        { name: "GOOGLE", image: "/temp-profile-pic.png", amount: 2800.75, change: -1.2 },
        { name: "AMZN", image: "/temp-profile-pic.png", amount: 3400.50, change: 3.1 },
    ];

    return (
        <div className="quick-actions flex">
            <p className="secondary-heading">Quick Actions</p>
            <div className="action-buttons flex gap-5">
                <Button label="Buy / Sell" onClick={() => {}} style={{width: "50%"}} />
                <Button label="Orders" onClick={() => {}} style={{width: "50%"}} />
            </div>
            <section className="top-movers">
                <div className="top-movers-header mb-2">
                    <p className="secondary-heading">Top Movers</p>
                    <button className="button color-green">View all</button>
                </div>
                <div className="top-movers-list flex flex-col gap-4">
                    {topMovers.slice(0, 2).map((mover, index) => (
                        <TopMover mover={mover} key={index} />
                    ))}
                </div>
            </section>
        </div>
    );
}

export default QuickActions;