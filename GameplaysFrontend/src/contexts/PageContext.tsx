import { createContext } from "react";
import { PageContextType } from "../types/ContextType";

const initialPageContext: PageContextType = {
    isLoginPageContext: false
}

export const PageContext = createContext<PageContextType>(initialPageContext);
