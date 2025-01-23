interface SearchResult {
    deck: string,
    id: number,
    image: {
        icon_url: string,
        tiny_url: string
    },
    name: string,
    original_release_date: string,
    platforms: {
        id: number,
        name: string
    }
}

export default SearchResult;
