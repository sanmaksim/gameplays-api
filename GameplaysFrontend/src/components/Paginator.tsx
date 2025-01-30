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
  for (let page = 1; page <= totalPages; page++) {
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
