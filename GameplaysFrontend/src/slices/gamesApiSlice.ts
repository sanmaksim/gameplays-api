import { apiSlice } from "./apiSlice";

const GAMES_URL = '/api/games';

export const gamesApiSlice = apiSlice.injectEndpoints({
    endpoints: (builder) => ({
        search: builder.mutation({
            query: (queryParams) => {
                const queryString = new URLSearchParams(queryParams).toString();
                return {
                    url: `${GAMES_URL}/search?${queryString}`,
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json'
                    }
                };
            }
        })
    })
});

export const { useSearchMutation } = gamesApiSlice;
