import { ActionMeta, CSSObjectWithLabel, MultiValue, OptionProps, SelectInstance, SingleValue } from 'react-select';
import { CSSProperties, KeyboardEvent, useEffect, useRef, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import AsyncSelect from 'react-select/async';
import debounce from 'lodash.debounce';
import type Option from '../types/OptionType';
import type Options from '../types/OptionsType';
import type SearchResults from '../types/SearchResultsType';

function SearchBar() {
    const navigate = useNavigate();

    let initSearchResults: SearchResults = {
        error: '',
        limit: 0,
        offset: 0,
        number_of_page_results: 0,
        number_of_total_results: 0,
        status_code: 0,
        results: [
            {
                deck: '',
                id: 0,
                image: {
                    icon_url: '',
                    tiny_url: ''
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
            console.error('Error fetching game data:', error);
            toast.error('Failed to fetch game data.');
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
                if (!searchResults) {
                    throw new Error('Error returning game data.');
                }

                // map search results to options
                const searchOptions: Options = searchResults.results.map((result) => ({
                    ...result,
                    isDivider: true,
                    label: result.name,
                    url: `/game/${result.id}`
                }));

                // Add "Show more results" to the options
                searchOptions.push({
                    ...initSearchResults.results[0],
                    isDivider: false,
                    label: "Show more results...",
                    url: `/search?q=${encodeURIComponent(inputString)}`
                });

                callback(searchOptions);
            } catch (error) {
                console.error('Error displaying results:', error);
                toast.error('Failed to display results.');
                callback([]);
            }
        }, 500)
    ).current;

    // Load options into react-select menu after debounce delay
    const loadOption = async (inputString: string): Promise<Options> => {
        return new Promise<Options>((callback) => {
            debouncedFetchGameData(inputString, callback);
        });
    };

    // custom react-select option component with custom styles
    const CustomOption = (props: OptionProps<Option>): JSX.Element => {
        const { data, innerRef, innerProps, getStyles }: OptionProps<Option> = props;
        
         // retrieve styles for the 'option' inner component
        const optionStyles = getStyles('option', props) as CSSProperties; // cast CSSObjectWithLabel to React.CSSProperties for compatibility

        let formattedDate: string;
        if (data.original_release_date) {
            const dateString: string = data.original_release_date;
            const date: Date = new Date(dateString);
            formattedDate = new Intl.DateTimeFormat('en-US', { month: 'short', day: '2-digit', year: 'numeric' }).format(new Date(date));
        }

        return (
            <>
                <div className="d-flex" ref={innerRef} {...innerProps} style={{ ...optionStyles }}>
                    {data.image.tiny_url ? (
                        <>
                            <img src={data.image.tiny_url} alt={data.label} className="me-2" />
                            <div>
                                <Link to={data.url} style={{ textDecoration: 'none', color: 'inherit' }}>{data.label}</Link> - {formattedDate!}
                            </div>
                        </>
                    ) : (
                        <div>
                            <Link to={data.url} style={{ textDecoration: 'none', color: 'inherit' }}>{data.label}</Link>
                        </div>
                    )}
                </div>
                {data.isDivider && <hr style={{ margin: 0 }} />}
            </>
        );
    };

    const customStyles = {
        menu: (menuStyles: CSSObjectWithLabel) => ({
            ...menuStyles,
            maxHeight: 'none' // remove the height limit for the menu
        }),
        menuList: (menuListStyles: CSSObjectWithLabel) => ({
            ...menuListStyles,
            maxHeight: 'none' // ensure the inner list expands too
        }),
        option: (optionStyles: CSSObjectWithLabel, state: { isFocused: boolean }) => ({
            ...optionStyles,
            backgroundColor: state.isFocused ? 'black' : 'white',
            color: state.isFocused ? 'white' : 'black',
            cursor: 'pointer',
            padding: '5px',
            textDecoration: state.isFocused ? 'underline' : 'none',
            transition: 'background-color 0.2s ease'
        })
    };

    return (
        <AsyncSelect
            className="w-50"
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
            styles={customStyles}
            value={option}
        />
    )
}

export default SearchBar;
