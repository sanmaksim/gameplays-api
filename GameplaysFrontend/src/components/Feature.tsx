function Feature() {
    return (
        <div className="container px-4 py-5">
            <div className="row g-4 py-5 row-cols-1 row-cols-lg-3">
                <div className="feature col">
                    <div className="feature-icon d-inline-flex align-items-center justify-content-center text-bg-primary bg-gradient fs-2 mb-3">
                        <svg className="bi" width="1em" height="1em"><use href="#collection"/></svg>
                    </div>
                    <h3 className="fs-2 text-body-emphasis">Import from Steam</h3>
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
                    <h3 className="fs-2 text-body-emphasis">Share with friends</h3>
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
                    <h3 className="fs-2 text-body-emphasis">Download the app</h3>
                    <p>Coming soon!</p>
                    <a href="#" className="icon-link">
                        Call to action
                        <svg className="bi"><use href="#chevron-right"/></svg>
                    </a>
                </div>
            </div>
        </div>
    )
}

export default Feature;
