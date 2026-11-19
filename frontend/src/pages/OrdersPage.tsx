import React, { useEffect, useState } from 'react';
import api from '../api/axiosClient';
import type { Order } from '../types';

export const OrdersPage: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get<Order[]>('/orders/my')
      .then((res) => setOrders(res.data))
      .finally(() => setLoading(false));
  }, []);

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-slate-900 mb-6">My Order History</h1>

      {loading ? (
        <p className="text-slate-500">Loading orders...</p>
      ) : orders.length === 0 ? (
        <p className="text-slate-500">You have not placed any orders yet.</p>
      ) : (
        <div className="space-y-4">
          {orders.map((order) => (
            <div key={order.id} className="bg-white p-5 rounded-xl shadow-sm border border-slate-200">
              <div className="flex flex-wrap items-center justify-between gap-2 pb-3 border-b border-slate-100">
                <div>
                  <span className="font-bold text-indigo-600">{order.orderNumber}</span>
                  <span className="text-xs text-slate-400 ml-3">
                    {new Date(order.createdAt).toLocaleString()}
                  </span>
                </div>
                <div className="flex items-center gap-3">
                  <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800">
                    {order.status}
                  </span>
                  <span className="font-bold text-slate-900">${order.totalAmount.toFixed(2)}</span>
                </div>
              </div>

              <div className="mt-3 space-y-1 text-sm text-slate-600">
                {order.items.map((item) => (
                  <div key={item.productId} className="flex justify-between">
                    <span>{item.productName} × {item.quantity}</span>
                    <span>${item.subTotal.toFixed(2)}</span>
                  </div>
                ))}
              </div>
              <p className="mt-3 text-xs text-slate-400">Shipped to: {order.shippingAddress}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

