import { Link } from 'react-router-dom';
import './Navbar.css';


const Navbar = () => {
  return (
        <nav className="navbar">
            <div className="nav-brand">
                <img src="/favicon.png" alt="Logo" className="nav-logo" />
                <p className="brand-name">StockSense</p>
            </div>
            <div className="nav-links">
                <ul className="main-links">
                    <li><Link to="/">Dashboard</Link></li>
                    <li><Link to="/search">Search</Link></li>
                    <li><Link to="/watchlist">Watchlist</Link></li>
                    <li><Link to="/compare">Compare</Link></li>
                    <li><Link to="/alerts">Alerts</Link></li>
                </ul>
                <ul className="secondary-links">
                    <li><Link to="/settings">Settings</Link></li>
                    <li><Link to="/help">Help / FAQ</Link></li>
                </ul>
            </div>

            <section className='profile-section'>
                <img src="/temp-profile-pic.png" alt="Profile" className="profile-pic" />
                <div className='profile-name'>
                    <p className='profile-name-text'>Pratap Tyagi</p>
                    <p className='profile-type'>
                        Pro User
                    </p>
                </div>
            </section>
        </nav>
    )
}

export default Navbar