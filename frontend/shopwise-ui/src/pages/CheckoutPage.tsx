import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';
import { useCart } from '../hooks/useCart';
import { useAuth } from '../store/AuthContext';
import type { Order } from '../types';

export default function CheckoutPage() {
  const { user } = useAuth();
  const { cart } = useCart(user ? 1 : null);
  const navigate = useNavigate();
  const [error, setError] = useState('');

  const placeOrder = async () => {
    try {
      const res = await api.post<Order>('/orders');
      navigate(`/orders/${res.data.id}`);
    } catch {
      setError('Failed to place order. Please try again.');
    }
  };

  if (!user) return <p>Please log in.</p>;
  if (!cart || cart.items.length === 0) return <p>Your cart is empty.</p>;

  return (
    <div className="checkout-page">
      <h2>Checkout</h2>
      {error && <p className="error">{error}</p>}
      <div className="order-summary">
        <h3>Order Summary</h3>
        {cart.items.map(item => (
          <div key={item.productId} className="summary-row">
            <span>{item.productName} × {item.quantity}</span>
            <span>${item.subtotal.toFixed(2)}</span>
          </div>
        ))}
        <div className="summary-total">
          <strong>Total: ${cart.total.toFixed(2)}</strong>
        </div>
      </div>
      <button onClick={placeOrder}>Place Order</button>
    </div>
  );
}
