import { useState } from "react"
import { Routes, Route } from "react-router-dom"
import Topbar from "./screens/global/Topbar"
import Sidebar from "./screens/global/Sidebar"
import Dashboard from "./screens/dashboard"
import Team from "./screens/users"
import UserDetail from "./pages/UserDetail"
import Movies from "./screens/movies"
import MovieDetail from "./pages/MovieDetail"
import MovieCreate from "./pages/MovieCreate"
import Persons from "./screens/persons"
import PersonCreate from "./pages/PersonCreate"
import PersonEdit from "./pages/PersonEdit"
import PersonDetail from "./pages/PersonDetail"
import Invoices from "./screens/invoices"
import Contacts from "./screens/contacts"
import Bar from "./screens/bar"
import Form from "./screens/form"
import Line from "./screens/line"
import Pie from "./screens/pie"
import FAQ from "./screens/faq"
import Geography from "./screens/geography"
import Calendar from "./screens/calendar/calendar"
import Login from "./pages/Login"
import Register from "./pages/Register"
import Profile from "./pages/Profile"
import { CssBaseline, ThemeProvider } from "@mui/material"
import { ColorModeContext, useMode } from "./theme"
import MovieEdit from "./pages/MovieEdit"
import UpdateUserProfile from "./screens/users/updateUser"
import Permissions from "./screens/permissions/listpermissions"
import Roles from "./screens/permissions/listRoles"
import RoleDetail from "./screens/permissions/roleDetail"
import Regions from "./screens/movies/region"
import Tags from "./screens/movies/tag"
import Plans from "./screens/plans";
import PlanCreate from "./pages/PlanCreate";
import PlanEdit from "./pages/PlanEdit";
import PlanDetail from "./pages/PlanDetail";
import Prices from "./screens/prices";
import PriceCreate from "./pages/PriceCreate";
import PriceEdit from "./pages/PriceEdit";
import PriceDetail from "./pages/PriceDetail";

function App() {
    const [theme, colorMode] = useMode()
    const [isSidebar, setIsSidebar] = useState(true)
    const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false)

    return (
        <ColorModeContext.Provider value={colorMode}>
            <ThemeProvider theme={theme}>
                <CssBaseline />
                <Routes>
                    {/* Public Routes */}
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />

                    {/* Protected Routes with Layout */}
                    <Route
                        path="/*"
                        element={
                            <div
                                className={`app ${
                                    isSidebarCollapsed
                                        ? "sidebar-collapsed"
                                        : ""
                                }`}
                            >
                                <Sidebar
                                    isSidebar={isSidebar}
                                    onCollapsedChange={setIsSidebarCollapsed}
                                />
                                <main className="content">
                                    <Topbar setIsSidebar={setIsSidebar} />
                                    <Routes>
                                        <Route
                                            path="/"
                                            element={<Dashboard />}
                                        />

                                        {/* User Management */}
                                        <Route
                                            path="/users"
                                            element={<Team />}
                                        />
                                        <Route
                                            path="/users/:userId"
                                            element={<UserDetail />}
                                        />
                                        <Route 
                                            path="/users/update/:userId" 
                                            element={<UpdateUserProfile/>} />
                                        {/*Permission Management */}
                                        <Route
                                            path="/permissions"
                                            element={<Permissions />}
                                        />
                                        <Route
                                            path="/roles"
                                            element={<Roles />}
                                        />
                                        <Route path="/roles/:roleId" element={<RoleDetail />} />
                                        {/* Movie Management */}
                                        <Route
                                            path="/movies"
                                            element={<Movies />}
                                        />
                                        <Route
                                            path="/movies/create"
                                            element={<MovieCreate />}
                                        />
                                        <Route
                                            path="/movies/:movieId"
                                            element={<MovieDetail />}
                                        />
                                        <Route
                                            path="/movies/edit/:movieID"
                                            element={<MovieEdit />}
                                        />
                                        {/* Region Management */}
                                        <Route
                                            path="/regions"
                                            element={<Regions />}
                                        />
                                        {/*Tag Management */}
                                        <Route
                                            path="/tags"
                                            element={<Tags />} 
                                        />
                                        {/* Person Management */}
                                        <Route
                                            path="/persons"
                                            element={<Persons />}
                                        />
                                        <Route
                                            path="/persons/create"
                                            element={<PersonCreate />}
                                        />
                                        <Route
                                            path="/persons/:personId"
                                            element={<PersonDetail />}
                                        />
                                        <Route
                                            path="/persons/edit/:personId"
                                            element={<PersonEdit />}
                                        />

                                        {/* Other Routes */}
                                        <Route
                                            path="/contacts"
                                            element={<Contacts />}
                                        />
                                        <Route
                                            path="/invoices"
                                            element={<Invoices />}
                                        />
                                        <Route
                                            path="/form"
                                            element={<Form />}
                                        />
                                        <Route
                                            path="/profile"
                                            element={<Profile />}
                                        />
                                        <Route path="/bar" element={<Bar />} />
                                        <Route path="/pie" element={<Pie />} />
                                        <Route
                                            path="/line"
                                            element={<Line />}
                                        />
                                        <Route path="/faq" element={<FAQ />} />
                                        <Route
                                            path="/calendar"
                                            element={<Calendar />}
                                        />
                                        <Route
                                            path="/geography"
                                            element={<Geography />}
                                        />
                                        {/* Plan Management */}
                                          <Route path="/plans" element={<Plans />} />
                                          <Route path="/plans/create" element={<PlanCreate />} />
                                          <Route path="/plans/:planId" element={<PlanDetail />} />
                                          <Route path="/plans/edit/:planId" element={<PlanEdit />} />

                                          {/* Price Management */}
                                          <Route path="/prices" element={<Prices />} />
                                          <Route path="/prices/create" element={<PriceCreate />} />
                                          <Route path="/prices/:priceId" element={<PriceDetail />} />
                                          <Route path="/prices/edit/:priceId" element={<PriceEdit />} />
                                    </Routes>
                                </main>
                            </div>
                        }
                    />
                </Routes>
            </ThemeProvider>
        </ColorModeContext.Provider>
    )
}

export default App