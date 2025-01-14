interface SearchResults {
    results: [
        {
            deck: string,
            id: number,
            image: {
                icon_url: string
            },
            name: string,
            original_release_date: string,
            platforms: {
                id: number,
                name: string
            },
            site_detail_url: string
        }
    ]
}

export default SearchResults;
