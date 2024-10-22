import { Outlet } from "react-router-dom";
import AuthProvider from "../providers/AuthProvider";
import Footer from "../components/Footer";
import NavBar from "../components/NavBar";

function MainLayout() {
    return (
        <AuthProvider>
            {/* d-flex: sets the display property to flexbox, allowing the container to layout its children in a flexible way. */}
            {/* flex-column: specifies that the flexbox layout should be in a column direction, meaning the children will be stacked vertically. */}
            {/* These two classes together create a flexible column layout, where the children (NavBar, Outlet, and Footer) will be stacked on top of each other. */}
            {/* The min-vh-100 class sets the minimum height of the container to 100% of the viewport height, ensuring that the container takes up at least the full height of the screen. */}
            <div className="d-flex flex-column min-vh-100">
                <NavBar />
                <Outlet />
                <Footer />
            </div>
        </AuthProvider>
    );
}

export default MainLayout;
