import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import api from '../services/api';
import ProductList from '../components/ProductList';
import SearchBar from '../components/SearchBar';
import { useCart } from '../hooks/useCart';
import { useAuth } from '../store/AuthContext';
import type { Product } from '../types';

// Full catalogue, with a search bar. The search query is mirrored to the URL
// (`?search=…`) so a search is shareable and survives reload.
export default function ProductsPage() {
  const [params, setParams] = useSearchParams();
  const search = params.get('search') ?? '';
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const { user } = useAuth();
  const { addItem } = useCart(user?.email ? 1 : null);

  useEffect(() => {
    setLoading(true);
    api.get<Product[]>('/products', { params: { search: search || undefined } })
      .then(res => setProducts(res.data))
      .finally(() => setLoading(false));
  }, [search]);

  const handleAddToCart = async (product: Product) => {
    if (!user) {
      alert('Please log in to add items to your cart.');
      return;
    }
    await addItem(product.id, 1);
    alert(`${product.name} added to cart!`);
  };

  return (
    <div className="products-page">
      <SearchBar
        value={search}
        onChange={v =>
          setParams(v ? { search: v } : {}, { replace: true })
        }
      />
      {loading ? (
        <p>Loading...</p>
      ) : (
        <ProductList products={products} onAddToCart={handleAddToCart} />
      )}
    </div>
  );
}
