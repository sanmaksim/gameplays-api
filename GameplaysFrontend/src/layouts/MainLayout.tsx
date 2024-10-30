import { Container } from "react-bootstrap";
import { Outlet } from "react-router-dom";
import AuthProvider from "../providers/AuthProvider";
import Footer from "../components/Footer";
import NavBar from "../components/NavBar";
import PageProvider from "../providers/PageProvider";

function MainLayout() {
    return (
        <AuthProvider>
            <PageProvider>
                <Container fluid="true">
                    <NavBar />
                    <Outlet />
                    <Footer />
                </Container>
            </PageProvider>
        </AuthProvider>
    );
}

export default MainLayout;
