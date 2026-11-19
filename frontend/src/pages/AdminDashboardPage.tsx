import React, { useEffect, useState } from 'react';
import api from '../api/axiosClient';
import type { Order, Product, SalesSummary, TopProductReport } from '../types';

export const AdminDashboardPage: React.FC = () => {
  const [summary, setSummary] = useState<SalesSummary | null>(null);
  const [topProducts, setTopProducts] = useState<TopProductReport[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [products, setProducts] = useState<Product[]>([]);

  const [name, setName] = useState('');
  const [category, setCategory] = useState('Electronics');
  const [sku, setSku] = useState('');
  const [price, setPrice] = useState('');
  const [stock, setStock] = useState('');

  const loadDashboard = async () => {
    const [sumRes, topRes, ordRes, prodRes] = await Promise.all([
      api.get<SalesSummary>('/reports/sales-summary'),
      api.get<TopProductReport[]>('/reports/top-products'),
      api.get<Order[]>('/orders'),
      api.get<Product[]>('/products'),
    ]);
    setSummary(sumRes.data);
    setTopProducts(topRes.data);
    setOrders(ordRes.data);
    setProducts(prodRes.data);
  };

  useEffect(() => {
    loadDashboard();
  }, []);

  const handleCreateProduct = async (e: React.FormEvent) => {
    e.preventDefault();
    await api.post('/products', {
      name,
      description: 'Added via Admin Dashboard',
      category,
      sku,
      price: Number(price),
      stockQuantity: Number(stock),
    });
    setName('');
    setSku('');
    setPrice('');
    setStock('');
    loadDashboard();
  };

  const handleStatusChange = async (orderId: string, status: number) => {
    await api.patch(`/orders/${orderId}/status`, { status });
    loadDashboard();
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 space-y-8">
      <h1 className="text-2xl font-bold text-slate-900">Admin Operations & SQL Reports</h1>

      {/* SQL Stored Procedure KPIs */}
      {summary && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <p className="text-xs text-slate-500">Total Revenue (SP)</p>
            <p className="text-2xl font-bold text-emerald-600">${summary.totalRevenue.toFixed(2)}</p>
          </div>
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <p className="text-xs text-slate-500">Completed Orders</p>
            <p className="text-2xl font-bold text-slate-800">{summary.completedOrders}</p>
          </div>
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <p className="text-xs text-slate-500">Cancelled Orders</p>
            <p className="text-2xl font-bold text-rose-600">{summary.cancelledOrders}</p>
          </div>
          <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
            <p className="text-xs text-slate-500">Avg Order Value</p>
            <p className="text-2xl font-bold text-indigo-600">${summary.averageOrderValue.toFixed(2)}</p>
          </div>
        </div>
      )}

      {/* Top Selling Products (Stored Procedure) + Add New Product */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
          <h2 className="font-bold text-lg mb-4">Top Selling Products (Stored Procedure)</h2>
          {topProducts.length === 0 ? (
            <p className="text-sm text-slate-500">Place an order to populate stored procedure analytics.</p>
          ) : (
            <div className="space-y-2 text-sm">
              {topProducts.map((tp) => (
                <div key={tp.productId} className="flex justify-between p-2.5 bg-slate-50 rounded-lg">
                  <span className="font-medium">{tp.productName}</span>
                  <span>{tp.totalUnitsSold} sold (${tp.totalRevenue.toFixed(2)})</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <form onSubmit={handleCreateProduct} className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm space-y-3">
          <h2 className="font-bold text-lg">Add New Product ({products.length} Active)</h2>
          <div className="grid grid-cols-2 gap-3">
            <input required placeholder="Product Name" value={name} onChange={(e) => setName(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
            <input required placeholder="SKU (e.g. TECH-99)" value={sku} onChange={(e) => setSku(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
            <input required placeholder="Category" value={category} onChange={(e) => setCategory(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
            <input required type="number" step="0.01" placeholder="Price ($)" value={price} onChange={(e) => setPrice(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
            <input required type="number" placeholder="Initial Stock" value={stock} onChange={(e) => setStock(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
            <button type="submit" className="bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold cursor-pointer">
              + Create Product
            </button>
          </div>
        </form>
      </div>

      {/* Manage All Customer Orders */}
      <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm overflow-x-auto">
        <h2 className="font-bold text-lg mb-4">All Customer Orders</h2>
        <table className="w-full text-sm text-left">
          <thead className="bg-slate-50 text-slate-600">
            <tr>
              <th className="p-3">Order #</th>
              <th className="p-3">Customer</th>
              <th className="p-3">Total</th>
              <th className="p-3">Status</th>
              <th className="p-3">Update Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {orders.map((o) => (
              <tr key={o.id}>
                <td className="p-3 font-medium text-indigo-600">{o.orderNumber}</td>
                <td className="p-3">{o.customerName}</td>
                <td className="p-3 font-semibold">${o.totalAmount.toFixed(2)}</td>
                <td className="p-3">{o.status}</td>
                <td className="p-3">
                  <select
                    disabled={o.status === 'Cancelled'}
                    defaultValue=""
                    onChange={(e) => handleStatusChange(o.id, Number(e.target.value))}
                    className="border rounded px-2 py-1 text-xs bg-white"
                  >
                    <option value="" disabled>Change...</option>
                    <option value={3}>Mark Shipped</option>
                    <option value={4}>Mark Delivered</option>
                    <option value={5}>Cancel (Restores Stock)</option>
                  </select>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
