import { useState, useEffect } from 'react';
import api from '../services/api';
import type { Cart } from '../types';

export function useCart(userId: number | null) {
  const [cart, setCart] = useState<Cart | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (!userId) return;
    setLoading(true);
    api.get<Cart>('/cart')
      .then(res => setCart(res.data))
      .catch(() => setCart(null))
      .finally(() => setLoading(false));
  }, []);

  const addItem = async (productId: number, quantity: number) => {
    const res = await api.post<Cart>('/cart/items', { productId, quantity });
    setCart(res.data);
  };

  const removeItem = async (productId: number) => {
    await api.delete(`/cart/items/${productId}`);
    setCart(prev =>
      prev ? { ...prev, items: prev.items.filter(i => i.productId !== productId) } : null
    );
  };

  return { cart, loading, addItem, removeItem, setCart };
}
