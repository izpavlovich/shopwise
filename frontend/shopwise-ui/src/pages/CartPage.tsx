import { useNavigate } from 'react-router-dom';
import { useCart } from '../hooks/useCart';
import { useAuth } from '../store/AuthContext';
import type { Cart, CartItem } from '../types';

function addItemLocally(cart: Cart, item: CartItem): Cart {
  cart.items.push(item);
  return cart;
}

export default function CartPage() {
  const { user } = useAuth();
  const { cart, loading, removeItem, setCart } = useCart(user ? 1 : null);
  const navigate = useNavigate();

  if (!user) return <p>Please log in to view your cart.</p>;
  if (loading) return <p>Loading cart...</p>;
  if (!cart || cart.items.length === 0) return <p>Your cart is empty.</p>;

  const handleDemoMutation = () => {
    if (!cart) return;
    const fakeItem: CartItem = { productId: 999, productName: 'Demo', unitPrice: 0, quantity: 1, subtotal: 0 };
    const updated = addItemLocally(cart, fakeItem);
    setCart(updated);
  };

  return (
    <div className="cart-page">
      <h2>Your Cart</h2>
      <table>
        <thead>
          <tr>
            <th>Product</th>
            <th>Price</th>
            <th>Qty</th>
            <th>Subtotal</th>
            <th>Action</th>
          </tr>
        </thead>
        <tbody>
          {cart.items.map(item => (
            <tr key={item.productId}>
              <td>{item.productName}</td>
              <td>${item.unitPrice.toFixed(2)}</td>
              <td>{item.quantity}</td>
              <td>${item.subtotal.toFixed(2)}</td>
              <td>
                <button onClick={() => removeItem(item.productId)}>Remove</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <div className="cart-footer">
        <strong>Total: ${cart.total.toFixed(2)}</strong>
        <button onClick={() => navigate('/checkout')}>Proceed to Checkout</button>
      </div>
    </div>
  );
}
