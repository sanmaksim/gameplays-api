import { createContext } from "react";
import { AuthContextType } from "../types/ContextType";

// default value when there is no matching context provider
const initialAuthContext: AuthContextType = {
    isLoggedIn: false,
    setLoggedIn: () => {
        throw new Error("login() not implemented");
    },
    setLoggedOut: () => {
        throw new Error("logout() not implemented");
    }
};

export const AuthContext = createContext<AuthContextType>(initialAuthContext);
