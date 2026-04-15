import { Link } from 'react-router-dom';
import type { Product } from '../types';

interface Props {
  product: Product;
  quantity: number;
  onAddToCart: (product: Product) => void;
}

export default function ProductCard({ product, quantity, onAddToCart }: Props) {
  const lineTotal = product.price * quantity;

  return (
    <div className="product-card">
      <Link to={`/products/${product.id}`}>
        <img src={product.imageUrl ?? 'https://picsum.photos/400/300'} alt={product.name} />
      </Link>
      <div className="product-info">
        <span className="category">{product.categoryName}</span>
        <Link to={`/products/${product.id}`}><h3>{product.name}</h3></Link>
        <p>{product.description}</p>
        <div className="price-row">
          <span className="price">${product.price.toFixed(2)}</span>
          {quantity > 1 && (
            <span className="line-total">Total: ${lineTotal}</span>
          )}
        </div>
        <span className={`stock ${product.stock === 0 ? 'out' : ''}`}>
          {product.stock > 0 ? `${product.stock} in stock` : 'Out of stock'}
        </span>
        <button
          onClick={() => onAddToCart(product)}
          disabled={product.stock === 0}
        >
          Add to Cart
        </button>
      </div>
    </div>
  );
}
