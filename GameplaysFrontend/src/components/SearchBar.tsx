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

    // track user input
    const [inputValue, setInputValue] = useState<string>('');

    // track AsyncSelect option
    const [option, setOption] = useState<SingleValue<Option> | null>(null);

    // track AsyncSelect options
    const [options, setOptions] = useState<Options>([]);

    // track search results
    let [searchResults, setSearchResults] = useState<SearchResults>(initSearchResults);

    // initialize ref object with the 'current' property set to null
    // ref selection is required for AsyncSelect's blur() and clearValue() methods
    const selectRef = useRef<SelectInstance<Option, boolean> | null>(null);

    // run post-render effects
    useEffect(() => {
        // reset the selected option on route changes
        setOption(null);
    }, [location]);

    // handle the current AsyncSelect option
    const handleChange = (currentOption: SingleValue<Option> | MultiValue<Option>, _actionMeta: ActionMeta<Option>): void => {
        setOption(null);
        setInputValue('');
        if (currentOption && 'url' in currentOption) {
            navigate(currentOption.url);
        }
    };

    // handle AsyncSelect keydown
    const handleKeyDown = (evt: KeyboardEvent): void => {
        if (evt.key === 'Enter') {
            evt.preventDefault();
            selectRef.current?.blur(); // close the options menu
            if (inputValue.trim()) {
                navigate(`/search?q=${encodeURIComponent(inputValue)}`);
            }
        }
    };

    // handle AsyncSelect input change
    const handleInputChange = (inputString: string): void => {
        setInputValue(inputString);
        if (inputString.trim() === '') {
            setOptions([]);
        }
    };

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
            console.error("Error fetching game data:", error);
            return searchResults;
        }
    };

    // convert debounced game data into AsyncSelect options
    const debouncedFetchGameData = useRef(
        debounce(async (inputString: string, callback: (options: Options) => void): Promise<void> => {
            try {
                if (inputString.trim() === '') {
                    callback([]); // clear options
                    return; // prevent API call
                }
        
                const searchResults = await fetchGameData(inputString);

                // map search results to options
                const searchOptions = searchResults.results.map((result) => ({
                    label: result.name,
                    url: `/game/${result.id}`,
                }));

                // Add "Show more results" to the options
                searchOptions.push({
                    label: "Show more results",
                    url: `/search?q=${encodeURIComponent(inputString)}`,
                });

                callback(searchOptions);
            } catch (error) {
                console.error("Error creating results:", error);
                callback([]);
            }
        }, 300)
    ).current;

    // Load options into react-select menu after debounce delay
    const loadOption = async (inputString: string): Promise<Options> => {
        return new Promise<Options>((callback) => {
            debouncedFetchGameData(inputString, callback);
        });
    };

    // custom select menu option styling
    const CustomOption = (props: OptionProps<Option>): JSX.Element => {
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
            onInputChange={handleInputChange}
            onKeyDown={handleKeyDown}
            openMenuOnClick={false}
            openMenuOnFocus={false}
            options={options}
            placeholder="Search Games"
            ref={selectRef} // react automatically sets the 'current' property of the ref to the instance of the AsyncSelect object after the component is mounted
            value={option}
        />
    )
}

export default SearchBar;
