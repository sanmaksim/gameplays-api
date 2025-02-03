import { Pagination } from "react-bootstrap";
import { useSearchParams } from "react-router-dom";
import type SearchResults from "../types/SearchResultsType";

type Props = {
  searchResults: SearchResults
}

function Paginator({ searchResults }: Props) {
  const [searchParams, setSearchParams] = useSearchParams();

  const currentPage: number = parseInt(searchParams.get('page') || '1', 10);
  const totalPages: number = Math.ceil(searchResults.number_of_total_results / searchResults.limit);

  let active = currentPage;
  let pages = [];

  const adjacent = 2;
  const visible = 5;

  // add elipsis on the left if active page 
  // moves past the center of the paginator
  if (active > (visible - adjacent)) {
    pages.push(<Pagination.Ellipsis />);
  }

  // handle paginator edges
  let adjacentLeft = adjacent;
  let adjacentRight = adjacent;
  if (active === adjacent) {
    adjacentLeft = adjacent - 1;
    adjacentRight = adjacent + 1;
  } else if (active === adjacent - 1) {
    adjacentLeft = adjacent - 2;
    adjacentRight = adjacent + 2;
  }
  if (active === totalPages - 1) {
    adjacentRight = adjacent - 1;
    adjacentLeft = adjacent + 1;
  } else if (active === totalPages) {
    adjacentRight = adjacent - 2;
    adjacentLeft = adjacent + 2;
  }

  // display pages based on active page and adjacency
  for (let page = (active - adjacentLeft); page <= (active + adjacentRight); page++) {
    pages.push(
      <Pagination.Item
        key={page}
        active={page === active}
        onClick={() => changePage(page)}
      >
        {page}
      </Pagination.Item>
    );
  }

  // add elipsis on the right if active page 
  // moves past the center of the paginator
  if (active < (totalPages - adjacent)) {
    pages.push(<Pagination.Ellipsis />);
  }

  const changePage = (newPage: number) => {
    const newParams = new URLSearchParams(searchParams);
    newParams.set('page', newPage.toString());
    setSearchParams(newParams);
  };

  return (
    <Pagination>
      <Pagination.First
        disabled={currentPage === 1}
        onClick={() => changePage(1)}
      />
      <Pagination.Prev
        disabled={currentPage === 1}
        onClick={() => changePage(currentPage - 1)}
      />
      {pages}
      <Pagination.Next
        disabled={currentPage === totalPages}
        onClick={() => changePage(currentPage + 1)}
      />
      <Pagination.Last
        disabled={currentPage === totalPages}
        onClick={() => changePage(totalPages)}
      />
    </Pagination>
  )
}

export default Paginator;
