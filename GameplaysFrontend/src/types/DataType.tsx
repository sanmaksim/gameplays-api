import React from "react";

type AuthProviderType = {
    children: React.ReactNode
}

type UserType = {
    userId?: number,
    username?: string,
    email?: string,
    password?: string
}

export type { AuthProviderType, UserType };
