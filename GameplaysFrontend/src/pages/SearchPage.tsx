import { Container, ListGroup } from "react-bootstrap";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";
import { useState } from "react";
import SearchResult from "../types/SearchResultType";
import SearchResults from "../types/SearchResultsType";

function SearchPage() {
    let initSearchResults: SearchResults = { 
        results: [
            {
                deck: '',
                id: 0,
                image: {
                    icon_url: ''
                },
                name: '',
                original_release_date: '',
                platforms: {
                    id: 0,
                    name: ''
                }
            }
        ]
    }

    let [searchResults, setSearchResults] = useState(initSearchResults);

    const fetchGameData = async (inputString: string): Promise<SearchResults> => {
        try {
            // proxy search query via server API (GiantBomb blocks client API calls)
            const response = await fetch(`https://localhost:5001/api/games/search?q=${inputString}`);

            if (!response.ok) {
                throw new Error(`Error: ${response.statusText}`);
            }

            const data = await response.json();

            setSearchResults(data);

            return data;
        } catch (error) {
            return searchResults;
        }
    };

    const [searchParams] = useSearchParams();
    const searchTerm = searchParams.get('q') || '';

    // fetch search results from API
    const {data, isLoading, error} = useQuery({
        queryKey: ['searchTerm', searchTerm],
        queryFn: () => fetchGameData(searchTerm)
    })

    return (
        <Container className="mt-4">
            {isLoading && <p>Loading...</p>}
            {error && <p>Error: {error.message}</p>}
            {data && (
                <ListGroup>
                    {data.results.map((result: SearchResult) => (
                        <ListGroup.Item key={result.id} className="d-flex">
                            <img src={result.image.icon_url} alt={result.name} className="me-2" />
                            <div>
                                <h6><strong>{result.name}</strong></h6>
                                <p>{result.deck}</p>
                            </div>
                        </ListGroup.Item>
                    ))}
                </ListGroup>
            )}
        </Container>
    )
}

export default SearchPage;
