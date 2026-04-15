import type { Product } from '../types';
import ProductCard from './ProductCard';

interface Props {
  products: Product[];
  onAddToCart: (product: Product) => void;
}

export default function ProductList({ products, onAddToCart }: Props) {
  return (
    <div className="product-grid">
      {products.map((product) => (
        <div>
          <ProductCard product={product} quantity={1} onAddToCart={onAddToCart} />
        </div>
      ))}
    </div>
  );
}
