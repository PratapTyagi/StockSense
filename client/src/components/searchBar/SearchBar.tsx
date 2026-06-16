import './SearchBar.css';

const SearchBar = () => {
    const placeholder = "Search for symbol, companies, indices, or ETFs...";
    return (
        <div className="search-bar">
            <div className='input-container'>
                <input placeholder={placeholder} className="search-input">
                </input>
                <span className="enter-button">
                    Enter
                </span>
            </div>
        </div>
    )
};

export default SearchBar;