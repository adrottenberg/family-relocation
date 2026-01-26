import { useState, useRef, useEffect } from 'react';
import { AutoComplete, Input, Typography } from 'antd';
import { EnvironmentOutlined } from '@ant-design/icons';

const { Text } = Typography;

interface AddressResult {
  street: string;
  street2?: string;
  city: string;
  state: string;
  zipCode: string;
  fullAddress: string;
}

interface MapboxFeature {
  id: string;
  place_name: string;
  text: string;
  properties: {
    address?: string;
  };
  context?: Array<{
    id: string;
    text: string;
    short_code?: string;
  }>;
}

interface AddressAutocompleteProps {
  onAddressSelect: (address: AddressResult) => void;
  placeholder?: string;
  defaultValue?: string;
}

const MAPBOX_TOKEN = import.meta.env.VITE_MAPBOX_TOKEN;

const AddressAutocomplete = ({
  onAddressSelect,
  placeholder = 'Enter address...',
  defaultValue = '',
}: AddressAutocompleteProps) => {
  const [options, setOptions] = useState<{ value: string; label: React.ReactNode; address: AddressResult }[]>([]);
  const [inputValue, setInputValue] = useState(defaultValue);
  const abortControllerRef = useRef<AbortController | null>(null);
  const debounceTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const parseMapboxResult = (feature: MapboxFeature): AddressResult => {
    const context = feature.context || [];

    // Extract address components from context
    const getContextValue = (prefix: string): string => {
      const item = context.find(c => c.id.startsWith(prefix));
      return item?.text || '';
    };

    const getStateCode = (): string => {
      const region = context.find(c => c.id.startsWith('region'));
      // Use short_code if available (e.g., "US-NJ" -> "NJ")
      if (region?.short_code) {
        const parts = region.short_code.split('-');
        return parts.length > 1 ? parts[1] : region.short_code;
      }
      return region?.text || '';
    };

    // Parse street from place_name (format: "368 Bergen Street, Newark, New Jersey 07103, United States")
    // The first part before the comma is the full street address
    const placeParts = feature.place_name.split(',');
    const street = placeParts[0]?.trim() || feature.text || '';

    return {
      street,
      city: getContextValue('place'),
      state: getStateCode(),
      zipCode: getContextValue('postcode'),
      fullAddress: feature.place_name,
    };
  };

  const formatSuggestionLabel = (_feature: MapboxFeature, address: AddressResult) => {
    // Format: "368 Bergen Street" on first line, "Newark, NJ 07103" on second
    const secondLine = [address.city, address.state, address.zipCode].filter(Boolean).join(', ');

    return (
      <div style={{ display: 'flex', alignItems: 'flex-start', gap: 10, padding: '4px 0' }}>
        <EnvironmentOutlined style={{ color: '#8c8c8c', marginTop: 3, fontSize: 14 }} />
        <div style={{ flex: 1, minWidth: 0 }}>
          <div style={{ fontWeight: 500, color: '#262626' }}>
            {address.street}
          </div>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {secondLine}
          </Text>
        </div>
      </div>
    );
  };

  const searchAddresses = async (query: string) => {
    if (!query || query.length < 3) {
      setOptions([]);
      return;
    }

    if (!MAPBOX_TOKEN) {
      console.warn('Mapbox token not configured. Add VITE_MAPBOX_TOKEN to .env.local');
      return;
    }

    // Cancel previous request
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
    }
    abortControllerRef.current = new AbortController();

    try {
      // Mapbox Geocoding API with autocomplete
      // Bias towards New Jersey, USA
      const params = new URLSearchParams({
        access_token: MAPBOX_TOKEN,
        country: 'US',
        types: 'address',
        autocomplete: 'true',
        limit: '5',
        // Bias towards Union County, NJ area
        proximity: '-74.35,40.67',
      });

      const url = `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(query)}.json?${params}`;

      const response = await fetch(url, {
        signal: abortControllerRef.current.signal,
      });

      if (!response.ok) {
        throw new Error(`Mapbox API error: ${response.status}`);
      }

      const data = await response.json();

      const suggestions = data.features.map((feature: MapboxFeature) => {
        const address = parseMapboxResult(feature);
        return {
          value: feature.place_name,
          label: formatSuggestionLabel(feature, address),
          address,
        };
      });

      setOptions(suggestions);
    } catch (error) {
      if ((error as Error).name !== 'AbortError') {
        console.error('Address search error:', error);
      }
    }
  };

  const handleSearch = (value: string) => {
    setInputValue(value);

    // Debounce the search
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }
    debounceTimerRef.current = setTimeout(() => {
      searchAddresses(value);
    }, 300);
  };

  const handleSelect = (_value: string, option: { address: AddressResult }) => {
    setInputValue(option.address.street);
    onAddressSelect(option.address);
    setOptions([]);
  };

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
      if (abortControllerRef.current) {
        abortControllerRef.current.abort();
      }
    };
  }, []);

  if (!MAPBOX_TOKEN) {
    // Return null if no token - will fall back to manual entry
    return null;
  }

  return (
    <AutoComplete
      value={inputValue}
      options={options}
      onSearch={handleSearch}
      onSelect={handleSelect}
      style={{ width: '100%' }}
      popupMatchSelectWidth={400}
      listHeight={300}
      dropdownStyle={{ padding: 0 }}
    >
      <Input
        placeholder={placeholder}
        suffix={<EnvironmentOutlined style={{ color: '#bfbfbf' }} />}
        allowClear
      />
    </AutoComplete>
  );
};

export default AddressAutocomplete;
