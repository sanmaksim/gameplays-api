import React from "react";

type ChildrenNodeType = {
    children: React.ReactNode
}

type UserType = {
    userId?: number,
    username?: string,
    email?: string,
    password?: string
}

export type { ChildrenNodeType, UserType };
