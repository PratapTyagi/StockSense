import { BrowserRouter, Route, Routes } from "react-router-dom"
import { Dashboard, Navbar, Search, Watchlist, Compare, Alerts, Settings, Help } from "../components"

const renderMainPage = (component: React.ReactNode) => {
    return (
        <div className="flex">
            <Navbar />
            {component}
        </div>
    )
}
export const AppRoutes = () => {
    return (
        <BrowserRouter>
            <Routes>
                <Route path='/' element={renderMainPage(<Dashboard />)} />
                <Route path='/search' element={renderMainPage(<Search />)} />
                <Route path='/watchlist' element={renderMainPage(<Watchlist />)} />
                <Route path='/compare' element={renderMainPage(<Compare />)} />
                <Route path='/alerts' element={renderMainPage(<Alerts />)} />
                <Route path='/settings' element={renderMainPage(<Settings />)} />
                <Route path='/help' element={renderMainPage(<Help />)} />
            </Routes>
        </BrowserRouter>
    )
}