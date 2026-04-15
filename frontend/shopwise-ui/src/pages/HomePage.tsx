import { useState, useEffect } from 'react';
import api from '../services/api';
import ProductList from '../components/ProductList';
import { useCart } from '../hooks/useCart';
import { useAuth } from '../store/AuthContext';
import type { Product } from '../types';

export default function HomePage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [search, setSearch] = useState('');
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
    <div className="home-page">
      <div className="search-bar">
        <input
          type="text"
          placeholder="Search products..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
      </div>
      {loading ? <p>Loading...</p> : <ProductList products={products} onAddToCart={handleAddToCart} />}
    </div>
  );
}
