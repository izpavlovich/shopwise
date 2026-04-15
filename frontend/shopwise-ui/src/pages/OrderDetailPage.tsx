import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import api from '../services/api';
import type { Order } from '../types';

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get<Order>(`/orders/${id}`)
      .then(res => setOrder(res.data))
      .catch(() => setOrder(null))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <p>Loading...</p>;
  if (!order) return <p>Order not found.</p>;

  return (
    <div className="order-detail">
      <h2>Order #{order.id}</h2>
      <p>Status: <span className={`status status-${order.status}`}>{order.status}</span></p>
      <p>Placed: {new Date(order.createdAt).toLocaleString()}</p>
      <table>
        <thead>
          <tr>
            <th>Product</th>
            <th>Qty</th>
            <th>Unit Price</th>
            <th>Subtotal</th>
          </tr>
        </thead>
        <tbody>
          {order.items.map(item => (
            <tr key={item.productId}>
              <td>{item.productName}</td>
              <td>{item.quantity}</td>
              <td>${item.unitPrice.toFixed(2)}</td>
              <td>${item.subtotal.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <p><strong>Total: ${order.totalAmount.toFixed(2)}</strong></p>
    </div>
  );
}
