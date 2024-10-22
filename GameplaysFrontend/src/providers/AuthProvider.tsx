import { useState } from "react";
import { AuthContext } from "../contexts/AuthContext";
import { AuthProviderType } from "../types/DataType";

function AuthProvider({ children }: AuthProviderType) {
    const [isLoggedIn, setIsLoggedIn ] = useState(false);

    const setLoggedIn = () => setIsLoggedIn(true);
    const setLoggedOut = () => setIsLoggedIn(false);

    return (
        <AuthContext.Provider value={{ isLoggedIn, setLoggedIn, setLoggedOut }}>
            { children }
        </AuthContext.Provider>
    );
}

export default AuthProvider ;
