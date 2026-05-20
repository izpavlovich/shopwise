import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { fetchPopular } from '../services/recommendations';
import RecommendationStrip from '../components/RecommendationStrip';
import SearchBar from '../components/SearchBar';
import type { RecommendedProduct } from '../types';

// Google-style landing: prominent search, then the "Popular right now"
// strip. The full catalogue lives on /products.
export default function HomePage() {
  const navigate = useNavigate();
  const [query, setQuery] = useState('');
  const [popular, setPopular] = useState<RecommendedProduct[]>([]);

  useEffect(() => {
    // Best-effort: a recommendations outage leaves the hero clean.
    fetchPopular().then(setPopular).catch(() => setPopular([]));
  }, []);

  const handleSubmit = () => {
    const q = query.trim();
    navigate(q ? `/products?search=${encodeURIComponent(q)}` : '/products');
  };

  return (
    <div className="home-page">
      <div className="home-hero">
        <SearchBar
          value={query}
          onChange={setQuery}
          onSubmit={handleSubmit}
          autoFocus
        />
      </div>
      <RecommendationStrip title="Popular right now" items={popular} />
    </div>
  );
}
