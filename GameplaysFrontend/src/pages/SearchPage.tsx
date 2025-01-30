import { Container, ListGroup } from "react-bootstrap";
import { toast } from "react-toastify";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";
import Paginator from "../components/Paginator";
import type SearchResult from "../types/SearchResultType";
import type SearchResults from "../types/SearchResultsType";

function SearchPage() {
    const [searchParams] = useSearchParams();
    const searchTerm = searchParams.get('q') || '';
    const searchPage = searchParams.get('page') || '';

    const fetchGameData = async (searchString: string, pageString: string): Promise<SearchResults> => {
        try {
            // proxy search query via server API (GiantBomb blocks client API calls)
            let response;
            if (pageString) {
                response = await fetch(`https://localhost:5001/api/games/search?q=${searchString}&page=${pageString}`);
            } else {
                response = await fetch(`https://localhost:5001/api/games/search?q=${searchString}`);
            }

            if (!response.ok) {
                throw new Error(`Error: ${response.statusText}`);
            }

            const data = await response.json();

            return data;
        } catch (error) {
            toast.error('Failed to fetch game data.');
            return {
                error: `${error}`,
                limit: 0,
                offset: 0,
                number_of_page_results: 0,
                number_of_total_results: 0,
                status_code: 0,
                results: []
            }
        }
    };

    // track query params and fetch data when they change
    const {data, isLoading, error} = useQuery({
        queryKey: ['query', searchTerm, searchPage],
        queryFn: () => fetchGameData(searchTerm, searchPage)
    })

    return (
        <>
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
            <br />
            <Container className="d-flex justify-content-center">
                {data && (
                    <Paginator searchResults={data} />
                )}
            </Container>
        </>
    )
}

export default SearchPage;
