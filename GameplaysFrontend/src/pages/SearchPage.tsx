import { Container, ListGroup } from "react-bootstrap";
import { useLocation } from "react-router-dom";
import SearchResult from "../types/SearchResultType";

function SearchPage() {
    const location = useLocation();

    return (
        <Container className="mt-4">
            {location.state.results.length > 0 ? (
                <ListGroup>
                    { location.state.results.map((result: SearchResult) => (
                        <ListGroup.Item key={result.id} className="d-flex">
                            <img src={result.image.icon_url} alt={result.name} className="me-2" />
                            <div>
                                <h6><strong>{result.name}</strong></h6>
                                <p>{result.deck}</p>
                            </div>
                        </ListGroup.Item>
                    )) }
                </ListGroup>
            ) : (
                <>
                    No results.
                </>
            )}
            {location.state.error && <p style={{ color: 'red' }}>{location.state.error}</p>}
        </Container>
    )
}

export default SearchPage;
