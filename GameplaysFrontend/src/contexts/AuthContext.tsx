import { createContext } from "react";
import { AuthContextType } from "../types/ContextType";

// default value when there is no matching context provider
const initialAuthContext: AuthContextType = {
    isLoggedInContext: false,
    setIsLoggedInContext: () => {
        throw new Error("setIsLoggedInContext() not implemented");
    },
    setIsLoggedOutContext: () => {
        throw new Error("setIsLoggedOutContext() not implemented");
    }
};

export const AuthContext = createContext<AuthContextType>(initialAuthContext);
