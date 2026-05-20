import axios from 'axios';
import type { RecommendedProduct } from '../types';

// The recommendations service is a separate (public, no-auth) Python service,
// proxied at /recommendations by both the Vite dev server and nginx in prod.
const recoApi = axios.create({ baseURL: '/recommendations' });

// The service speaks snake_case; map it to the camelCase used across the UI.
interface ApiItem {
  id: number;
  category_id: number;
  name: string;
  price: number;
  image_url: string | null;
  score?: number;
  units_sold?: number;
}

function toProduct(item: ApiItem): RecommendedProduct {
  return {
    id: item.id,
    categoryId: item.category_id,
    name: item.name,
    price: item.price,
    imageUrl: item.image_url,
    score: item.score,
    unitsSold: item.units_sold,
  };
}

export async function fetchPopular(limit = 6): Promise<RecommendedProduct[]> {
  const res = await recoApi.get<{ items: ApiItem[] }>('/popular', {
    params: { limit },
  });
  return res.data.items.map(toProduct);
}

export async function fetchRelated(
  productId: number,
  limit = 6,
): Promise<RecommendedProduct[]> {
  const res = await recoApi.get<{ items: ApiItem[] }>(
    `/related/${productId}`,
    { params: { limit } },
  );
  return res.data.items.map(toProduct);
}
