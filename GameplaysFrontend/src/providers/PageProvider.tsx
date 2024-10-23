import { ProviderType } from "../types/DataType";
import { PageContextType } from "../types/ContextType";
import { PageContext } from "../contexts/PageContext";
import { useEffect, useState } from "react";
import { useLocation } from "react-router-dom";

function PageProvider({ children }: ProviderType) {
    const [ isLoginPage, setIsLoginPage ] = useState(false);
    const [ isRegisterPage, setIsRegisterPage ] = useState(true);

    const location = useLocation();

    // toggle page context based on path
    useEffect(() => {
        if (location.pathname === '/user/login') {
            setIsLoginPage(true);
            setIsRegisterPage(false);
        } else if (location.pathname === '/user/register') {
            setIsLoginPage(false);
            setIsRegisterPage(true);
        } else {
            setIsLoginPage(false);
            setIsRegisterPage(false);
        }
    }, [location.pathname]);

    const pageContext: PageContextType = {
        isLoginPageContext: isLoginPage,
        isRegisterPageContext: isRegisterPage,
        setLoginPageContext: () => {
            setIsLoginPage(true);
            setIsRegisterPage(false);
        },
        setRegisterPageContext: () => {
            setIsLoginPage(false);
            setIsRegisterPage(true);
        }
    }

    return (
        <PageContext.Provider value={ pageContext }>
            { children }
        </PageContext.Provider>
    );
}

export default PageProvider;
