import { KeyboardEvent, useEffect, useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ActionMeta, MultiValue, OptionProps, SelectInstance, SingleValue } from 'react-select';
import AsyncSelect from 'react-select/async';
import debounce from 'lodash.debounce';
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

    // update user input
    const [inputValue, setInputValue] = useState<string>('');

    // update selected react-select option
    const [selectedOption, setSelectedOption] = useState<SingleValue<Option> | null>(null);

    // update search results
    let [searchResults, setSearchResults] = useState<SearchResults>(initSearchResults);

    // handle react-select option click
    const handleChange = (option: SingleValue<Option> | MultiValue<Option>, _actionMeta: ActionMeta<Option>) => {
        setSelectedOption(null);
        setInputValue('');
        if (option && 'url' in option) {
            navigate(option.url);
        }
    };

    useEffect(() => {
        // Reset the selected option on route changes
        setSelectedOption(null);
    }, [location]);

    // get search results
    const fetchGameData = async (inputString: string): Promise<SearchResults> => {
        try {
            // proxy search query via server API (GiantBomb blocks client API calls)
            const response = await fetch(`https://localhost:5001/api/games/search?q=${encodeURIComponent(inputString)}`);

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

    // convert debounced game data into options
    const debouncedFetchGameData = useRef(
        debounce(async (inputString: string, callback: (options: Options) => void) => {
            try {
                const searchResults = await fetchGameData(inputString);

                const options = searchResults.results.map((result) => ({
                    label: result.name,
                    url: `/game/${result.id}`
                }));

                // add a final option to end of list
                if (options) {
                    options.push({
                        label: 'Show more results',
                        url: `/search?q=${encodeURIComponent(inputString)}`
                    });
                }

                callback(options);
            } catch (error) {
                console.error("Error fetching game data:", error);
                callback([]);
            }
        }, 300)
    ).current;

    // load options into react-select menu after debounce delay
    const loadOption = async (inputString: string): Promise<Options> => {
        return new Promise<Options>((resolve) => {
            debouncedFetchGameData(inputString, resolve);
        });
    };

    // initialize ref object with the 'current' property set to null
    // ref object is required to trigger AsyncSelect's blur method
    const selectRef = useRef<SelectInstance<Option, boolean> | null>(null);

    // nav to search results page on Enter keydown
    const handleKeyDown = (evt: KeyboardEvent): void => {
        if (evt.key === 'Enter') {
            evt.preventDefault();
            if (selectRef.current) {
                selectRef.current.blur(); // close the options menu
            }
            if (inputValue.trim()) {
                navigate(`/search?q=${encodeURIComponent(inputValue)}`);
            }
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
            cacheOptions
            components={{
                DropdownIndicator: null,
                Option: CustomOption
            }}
            defaultOptions
            inputValue={inputValue}
            isSearchable={true}
            loadOptions={loadOption}
            loadingMessage={() => "Searching..."}
            noOptionsMessage={() => "No results"}
            onChange={handleChange}
            onKeyDown={handleKeyDown}
            onInputChange={(inputString) => { setInputValue(inputString) }}
            openMenuOnClick={false}
            openMenuOnFocus={false}
            placeholder="Search Games"
            ref={selectRef} // react automatically sets the 'current' property of the ref to the instance of the AsyncSelect object after the component is mounted
            value={selectedOption}
        />
    )
}

export default SearchBar;
