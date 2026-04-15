import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import api from '../services/api';
import { useCart } from '../hooks/useCart';
import { useAuth } from '../store/AuthContext';
import type { Product } from '../types';

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [product, setProduct] = useState<Product | null>(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const { user } = useAuth();
  const { addItem } = useCart(user ? 1 : null);

  useEffect(() => {
    api.get<Product>(`/products/${id}`)
      .then(res => setProduct(res.data))
      .catch(() => setProduct(null))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <p>Loading...</p>;
  if (!product) return <p>Product not found.</p>;

  const handleAddToCart = async () => {
    if (!user) { alert('Please log in.'); return; }
    await addItem(product.id, quantity);
    alert(`Added ${quantity} × ${product.name} to cart!`);
  };

  return (
    <div className="product-detail">
      <img src={product.imageUrl ?? ''} alt={product.name} />
      <div className="detail-info">
        <span className="category">{product.categoryName}</span>
        <h1>{product.name}</h1>
        <p>{product.description}</p>
        <p className="price">${product.price.toFixed(2)}</p>
        <p>{product.stock} in stock</p>
        <p className="line-total">Total: ${product.price * quantity}</p>
        <div className="quantity-row">
          <label>Qty:
            <input
              type="number"
              min={1}
              max={product.stock}
              value={quantity}
              onChange={e => setQuantity(Number(e.target.value))}
            />
          </label>
          <button onClick={handleAddToCart} disabled={product.stock === 0}>
            Add to Cart
          </button>
        </div>
      </div>
    </div>
  );
}
