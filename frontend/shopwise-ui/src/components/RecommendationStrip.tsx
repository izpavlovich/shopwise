import { Link } from 'react-router-dom';
import type { RecommendedProduct } from '../types';

interface Props {
  title: string;
  items: RecommendedProduct[];
}

// Lightweight "you may also like" strip. The recommendations payload is
// trimmed (no stock/description), so these tiles just link to the product
// page rather than reusing the full ProductCard.
export default function RecommendationStrip({ title, items }: Props) {
  if (items.length === 0) return null;

  return (
    <section className="reco-section">
      <h2>{title}</h2>
      <div className="reco-grid">
        {items.map((p) => (
          <Link key={p.id} to={`/products/${p.id}`} className="reco-card">
            <img
              src={p.imageUrl ?? 'https://picsum.photos/400/300'}
              alt={p.name}
            />
            <div className="reco-body">
              <h4>{p.name}</h4>
              <span className="price">${p.price.toFixed(2)}</span>
              {p.unitsSold !== undefined && (
                <span className="reco-meta">{p.unitsSold} sold</span>
              )}
            </div>
          </Link>
        ))}
      </div>
    </section>
  );
}
