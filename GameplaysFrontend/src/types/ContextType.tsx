type AuthContextType = {
    isLoggedInContext: boolean,
    setIsLoggedInContext: () => void,
    setIsLoggedOutContext: () => void
}

type PageContextType = {
    isLoginPageContext: boolean,
    isRegisterPageContext: boolean
    setLoginPageContext: () => void,
    setRegisterPageContext: () => void
}

export type { AuthContextType, PageContextType };
