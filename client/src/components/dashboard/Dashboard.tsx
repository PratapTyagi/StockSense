import AssetAllocation from "../asset-allocation/AssetAllocation";
import PerformanceOverview from "../performance-overview/PerformanceOverview";
import PortfolioOverview from "../portfolio-overview/PortfolioOverview"
import QuickActions from "../quick-actions/QuickActions";
import './Dashboard.css';

const Dashboard = () => {
    return (
        <div className="dashboard-container">
            <section className="dashboard-navbar" style={{width: "85vw", background: "white", padding:"20px", borderLeft: "1px solid red"}}>
                Navbar
            </section>
            <div className="dashboard-details">
                <PortfolioOverview />
                <QuickActions />
                <PerformanceOverview />
                <AssetAllocation />
            </div>
        </div>
    )
}

export default Dashboard