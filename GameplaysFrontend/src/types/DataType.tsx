import React from "react";

type ProviderType = {
    children: React.ReactNode
}

type UserType = {
    userId?: number,
    username?: string,
    email?: string,
    password?: string
}

export type { ProviderType, UserType };
