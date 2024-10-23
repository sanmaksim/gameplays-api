import { Link } from "react-router-dom";

function Hero() {
    return (
        <>
            {/* Hero */}
            <div className="px-4 py-5 my-5 text-center">
                <h1 className="display-5 fw-bold text-body-emphasis">Make your gameplays count</h1>
                <div className="col-lg-6 mx-auto">
                    <p className="lead mb-4">Keep track of your game collection, clear your backlog.</p>
                    <div className="d-grid gap-2 d-sm-flex justify-content-sm-center">
                        <Link to="/user/register">
                            <button type="button" className="btn btn-primary btn-lg px-4 gap-3">Sign up</button>
                        </Link>
                        <Link to="/user/login">
                            <button type="button" className="btn btn-secondary btn-lg px-4">Sign in</button>
                        </Link>
                    </div>
                </div>
            </div>

            {/* Feature */}
            <div className="container px-4 py-5">
                <div className="row g-4 py-5 row-cols-1 row-cols-lg-3">
                    <div className="feature col">
                        <div className="feature-icon d-inline-flex align-items-center justify-content-center text-bg-primary bg-gradient fs-2 mb-3">
                            <svg className="bi" width="1em" height="1em"><use href="#collection"/></svg>
                        </div>
                        <h3 className="fs-2 text-body-emphasis">Featured title</h3>
                        <p>Paragraph of text beneath the heading to explain the heading. We'll add onto it with another sentence and probably just keep going until we run out of words.</p>
                        <a href="#" className="icon-link">
                            Call to action
                            <svg className="bi"><use href="#chevron-right"/></svg>
                        </a>
                    </div>
                    <div className="feature col">
                        <div className="feature-icon d-inline-flex align-items-center justify-content-center text-bg-primary bg-gradient fs-2 mb-3">
                            <svg className="bi" width="1em" height="1em"><use href="#people-circle"/></svg>
                        </div>
                        <h3 className="fs-2 text-body-emphasis">Featured title</h3>
                        <p>Paragraph of text beneath the heading to explain the heading. We'll add onto it with another sentence and probably just keep going until we run out of words.</p>
                        <a href="#" className="icon-link">
                            Call to action
                            <svg className="bi"><use href="#chevron-right"/></svg>
                        </a>
                    </div>
                    <div className="feature col">
                        <div className="feature-icon d-inline-flex align-items-center justify-content-center text-bg-primary bg-gradient fs-2 mb-3">
                            <svg className="bi" width="1em" height="1em"><use href="#toggles2"/></svg>
                        </div>
                        <h3 className="fs-2 text-body-emphasis">Featured title</h3>
                        <p>Paragraph of text beneath the heading to explain the heading. We'll add onto it with another sentence and probably just keep going until we run out of words.</p>
                        <a href="#" className="icon-link">
                            Call to action
                            <svg className="bi"><use href="#chevron-right"/></svg>
                        </a>
                    </div>
                </div>
            </div>

        </>
    );
}

export default Hero;
