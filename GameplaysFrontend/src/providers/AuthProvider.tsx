import { useState } from "react";
import { AuthContext } from "../contexts/AuthContext";
import { ProviderType } from "../types/DataType";
import { AuthContextType } from "../types/ContextType";

function AuthProvider({ children }: ProviderType) {
    const [ isLoggedIn, setIsLoggedIn ] = useState(false);

    const authContext: AuthContextType = {
        isLoggedInContext: isLoggedIn,
        setIsLoggedInContext: () => setIsLoggedIn(true),
        setIsLoggedOutContext: () => setIsLoggedIn(false)
    }

    return (
        <AuthContext.Provider value= { authContext }>
            { children }
        </AuthContext.Provider>
    );
}

export default AuthProvider;
