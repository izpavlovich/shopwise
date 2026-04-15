import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import api from '../services/api';
import { useAuth } from '../store/AuthContext';
import type { Order } from '../types';

export default function OrdersPage() {
  const { user } = useAuth();
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user) return;
    api.get<Order[]>('/orders')
      .then(res => setOrders(res.data))
      .finally(() => setLoading(false));
  }, [user]);

  if (!user) return <p>Please log in to view orders.</p>;
  if (loading) return <p>Loading orders...</p>;
  if (orders.length === 0) return <p>No orders yet.</p>;

  return (
    <div className="orders-page">
      <h2>Your Orders</h2>
      <table>
        <thead>
          <tr>
            <th>Order #</th>
            <th>Date</th>
            <th>Status</th>
            <th>Total</th>
            <th>Items</th>
          </tr>
        </thead>
        <tbody>
          {orders.map(order => (
            <tr key={order.id}>
              <td><Link to={`/orders/${order.id}`}>#{order.id}</Link></td>
              <td>{new Date(order.createdAt).toLocaleDateString()}</td>
              <td><span className={`status status-${order.status}`}>{order.status}</span></td>
              <td>${order.totalAmount.toFixed(2)}</td>
              <td>{order.items.length} item(s)</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
