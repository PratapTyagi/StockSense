import { Button } from "../../ui";
import './PortfolioOverview.css';

const PortfolioOverview = () => {
  return (
    <div className="portfolio-overview">
        <section className="portfolio-section">
            <div className="portfolio-value-section">
                <p className="secondary-heading">Total Portfolio Value</p>
                <p className="portfolio-value">
                    <span className="portfolio-value-span">₹ 24,85,420.50</span>
                    <span className="profit-span">+2.4%</span>
                </p>
            </div>
            <div className="portfolio-actions">
                <Button label="Withdraw" onClick={() => {}} />
                <Button label="Add Funds" isPrimary onClick={() => {}} />
            </div> 
        </section>

        <div className="portfolio-details">
            <div className="investment-amount">
                <p className="secondary-heading">Investment amount</p>
                <p>₹ 18,50,000</p>
            </div>
            <div className="total-returns">
                <p className="secondary-heading">Total returns</p>
                <p className="color-green">₹ 6,35,420</p>
            </div>
            <div className="profit-loss">
                <p className="secondary-heading">Today's P&L</p>
                <p className="color-green">₹250.00 (5.00%)</p>
            </div>
        </div>
    </div>
  );
};

export default PortfolioOverview;