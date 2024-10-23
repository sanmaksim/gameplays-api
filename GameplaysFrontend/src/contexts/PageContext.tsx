import { createContext } from "react";
import { PageContextType } from "../types/ContextType";

const initialPageContext: PageContextType = {
    isLoginPageContext: false,
    isRegisterPageContext: false,
    setLoginPageContext: () => {
        throw new Error("setLoginPageContext() not implemented")
    },
    setRegisterPageContext: () => {
        throw new Error("setRegisterPageContext() not implemented")
    }
}

export const PageContext = createContext<PageContextType>(initialPageContext);
