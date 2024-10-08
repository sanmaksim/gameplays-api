import { Link } from 'react-router-dom';

function Footer() {
    const currentYear = new Date().getFullYear();

    return (
        <div className="container mt-auto">
            <footer className="d-flex flex-wrap justify-content-between align-items-center py-3 my-4 border-top">
                <p className="col-md-4 mb-0 text-body-secondary">&copy; {currentYear} Company, Inc</p>

                <a href="/" className="col-md-4 d-flex align-items-center justify-content-center mb-3 mb-md-0 me-md-auto link-body-emphasis text-decoration-none">
                    <svg className="bi me-2" width="40" height="32"><use href="#bootstrap" /></svg>
                </a>

                <ul className="nav col-md-4 justify-content-end">
                    <li className="nav-item">
                        <Link className="nav-link px-2 text-body-secondary" to="/about">About</Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link px-2 text-body-secondary" to="/help">Help</Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link px-2 text-body-secondary" to="/privacy">Privacy Policy</Link>
                    </li>
                    <li className="nav-item">
                        <Link className="nav-link px-2 text-body-secondary" to="/tos">Terms of Service</Link>
                    </li>
                </ul>
            </footer>
        </div>
    );
}

export default Footer;
