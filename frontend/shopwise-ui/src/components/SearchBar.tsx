import { FormEvent } from 'react';

interface Props {
  value: string;
  onChange: (value: string) => void;
  /** Called on Enter / icon click. Omit when the parent searches live. */
  onSubmit?: () => void;
  placeholder?: string;
  autoFocus?: boolean;
}

// Unified search pill used on both the landing hero and the products page:
// a single rounded input with an embedded magnifying-glass submit button.
export default function SearchBar({
  value,
  onChange,
  onSubmit,
  placeholder = 'Search products...',
  autoFocus = false,
}: Props) {
  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    onSubmit?.();
  };

  return (
    <form className="search-pill" onSubmit={handleSubmit} role="search">
      <input
        type="text"
        placeholder={placeholder}
        value={value}
        onChange={e => onChange(e.target.value)}
        autoFocus={autoFocus}
        aria-label="Search products"
      />
      <button type="submit" aria-label="Search">
        {/* Magnifying glass — kept inline so we don't pull in an icon dep. */}
        <svg viewBox="0 0 24 24" width="20" height="20" fill="none"
             stroke="currentColor" strokeWidth="2"
             strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <circle cx="11" cy="11" r="7" />
          <path d="M21 21l-4.3-4.3" />
        </svg>
      </button>
    </form>
  );
}
