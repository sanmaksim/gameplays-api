import { apiSlice } from "./apiSlice";

const GAMES_URL = '/api/games';

export const gamesApiSlice = apiSlice.injectEndpoints({
    endpoints: (builder) => ({
        search: builder.mutation({
            query: ({ queryParams, limit = "10" }: { queryParams: { q: string, page?: string }, limit?: string }) => {
                const queryString = new URLSearchParams(queryParams).toString();
                return {
                    url: `${GAMES_URL}/search?${queryString}`,
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                        'Result-Limit': limit
                    }
                };
            }
        })
    })
});

export const { useSearchMutation } = gamesApiSlice;
