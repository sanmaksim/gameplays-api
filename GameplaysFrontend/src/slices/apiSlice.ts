import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";

let serverUrl = "";

if (process.env.NODE_ENV === 'development') {
    serverUrl = "https://localhost:5001";
} else {
    serverUrl = "https://localhost:8001";
}

const baseQuery = fetchBaseQuery({ baseUrl: serverUrl });

export const apiSlice = createApi({
    baseQuery,
    tagTypes: ['User'],
    endpoints: () => ({})
});
