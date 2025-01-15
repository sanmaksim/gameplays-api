import { Link, useNavigate } from 'react-router-dom';
import { OptionProps } from 'react-select';
import { useState } from 'react';
import AsyncSelect from 'react-select/async';
import Option from '../types/OptionType';
import Options from '../types/OptionsType';
import SearchResults from '../types/SearchResultsType';

function SearchBar() {
    const navigate = useNavigate();
    
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

    // update search results
    let [searchResults, setSearchResults] = useState(initSearchResults);

    // update user input
    const [inputValue, setInputValue] = useState('');

    // get search results
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
            console.log(error);
            return searchResults;
        }
    };

    // convert results data into search menu options
    const loadOption = async (inputString: string): Promise<Options> => {
        let options: Options = [];

        try {
            searchResults = await fetchGameData(inputString);

            if (searchResults) {
                searchResults.results.forEach((result) => {
                    options.push({ 
                        label: result.name,
                        url: '/result'
                    });
                });

                // add a final option to end of list
                if (options) {
                    options.push({
                        label: 'Show more results',
                        url: `/search?q=${inputString}`
                    });
                }
            }

            return options;

        } catch (error) {
            console.log(error);
            return options;
        }
    };

    // nav to search results page on Enter keydown
    const handleKeyDown = (evt: React.KeyboardEvent): void => {
        if (evt.key === 'Enter') {
            evt.preventDefault();
            // need to handle this query on results page
            navigate(`/search?q=${inputValue}`, { state: searchResults });
        }
    };

    // custom select menu option styling
    const CustomOption = (props: OptionProps<Option>) => {
        const { data, innerRef, innerProps } = props;
        return (
            <div ref={innerRef} {...innerProps} style={{ padding: '5px', cursor: 'pointer' }}>
                <Link to={data.url}>{data.label}</Link>
            </div>
        );
    };

    return (
        <AsyncSelect 
            className="w-50" 
            components={{ 
                DropdownIndicator: null,
                Option: CustomOption
            }}
            inputValue={inputValue} 
            isSearchable={true} 
            loadOptions={loadOption} 
            loadingMessage={() => "Searching..."} 
            noOptionsMessage={() => "No results"} 
            onKeyDown={handleKeyDown} 
            onInputChange={(inputString) => {setInputValue(inputString)}}
            openMenuOnClick={false} 
            openMenuOnFocus={false} 
            placeholder="Search Games" 
        />
    )
}

export default SearchBar;
