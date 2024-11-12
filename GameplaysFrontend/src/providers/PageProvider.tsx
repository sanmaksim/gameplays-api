import { ChildrenNodeType } from "../types/DataType";
import { PageContextType } from "../types/ContextType";
import { PageContext } from "../contexts/PageContext";
import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";

function PageProvider({ children }: ChildrenNodeType) {
    const [ isLoginPage, setIsLoginPage ] = useState(false);

    const location = useLocation();

    useEffect(() => {
        if (location.pathname === '/login') {
            setIsLoginPage(true);
        } else {
            setIsLoginPage(false);
        }
    }, [location.pathname]);

    const pageContext: PageContextType = {
        isLoginPageContext: isLoginPage
    }

    return (
        <PageContext.Provider value={ pageContext }>
            { children }
        </PageContext.Provider>
    );
}

export default PageProvider;
