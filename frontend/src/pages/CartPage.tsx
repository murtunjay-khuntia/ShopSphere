import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Trash2, ShoppingBag } from 'lucide-react';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import api from '../api/axiosClient';

export const CartPage: React.FC = () => {
  const { items, updateQuantity, removeFromCart, clearCart, totalAmount } = useCart();
  const { user } = useAuth();
  const navigate = useNavigate();

  const [shippingAddress, setShippingAddress] = useState('Plot 42, HITEC City, Hyderabad, Telangana');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const handleCheckout = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return navigate('/login');

    setSubmitting(true);
    setError('');
    try {
      await api.post('/orders', {
        shippingAddress,
        items: items.map((i) => ({ productId: i.product.id, quantity: i.quantity })),
      });
      clearCart();
      navigate('/orders');
    } catch (err: any) {
      setError(err.response?.data?.detail || 'Order failed. Please check stock availability.');
    } finally {
      setSubmitting(false);
    }
  };

  if (items.length === 0) {
    return (
      <div className="max-w-3xl mx-auto px-4 py-16 text-center">
        <ShoppingBag className="w-12 h-12 text-slate-400 mx-auto mb-3" />
        <h2 className="text-2xl font-bold text-slate-800">Your cart is empty</h2>
        <button
          onClick={() => navigate('/')}
          className="mt-4 bg-indigo-600 text-white px-5 py-2 rounded-lg hover:bg-indigo-700 cursor-pointer"
        >
          Browse Products
        </button>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold text-slate-900 mb-6">Shopping Cart & Checkout</h1>

      {error && (
        <div className="bg-rose-50 text-rose-700 p-3 rounded-lg mb-4 border border-rose-200">{error}</div>
      )}

      <div className="bg-white rounded-xl shadow-sm border border-slate-200 divide-y divide-slate-200 mb-6">
        {items.map(({ product, quantity }) => (
          <div key={product.id} className="p-4 flex items-center justify-between gap-4">
            <div>
              <h3 className="font-semibold text-slate-800">{product.name}</h3>
              <p className="text-sm text-slate-500">${product.price.toFixed(2)} each</p>
            </div>

            <div className="flex items-center gap-3">
              <input
                type="number"
                min={1}
                max={product.stockQuantity}
                value={quantity}
                onChange={(e) => updateQuantity(product.id, Number(e.target.value))}
                className="w-16 border border-slate-300 rounded px-2 py-1 text-center"
              />
              <span className="font-semibold w-20 text-right">
                ${(product.price * quantity).toFixed(2)}
              </span>
              <button
                onClick={() => removeFromCart(product.id)}
                className="text-rose-500 hover:text-rose-700 cursor-pointer"
              >
                <Trash2 className="w-5 h-5" />
              </button>
            </div>
          </div>
        ))}
      </div>

      <form onSubmit={handleCheckout} className="bg-white p-6 rounded-xl shadow-sm border border-slate-200">
        <div className="mb-4">
          <label className="block text-sm font-medium text-slate-700 mb-1">Shipping Address</label>
          <textarea
            required
            rows={2}
            value={shippingAddress}
            onChange={(e) => setShippingAddress(e.target.value)}
            className="w-full border border-slate-300 rounded-lg p-2.5 text-sm"
          />
        </div>

        <div className="flex items-center justify-between pt-4 border-t border-slate-100">
          <span className="text-xl font-bold text-slate-900">Total: ${totalAmount.toFixed(2)}</span>
          <button
            type="submit"
            disabled={submitting}
            className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold px-6 py-2.5 rounded-lg transition cursor-pointer disabled:opacity-50"
          >
            {submitting ? 'Placing Order...' : user ? 'Place Atomic Order' : 'Sign In to Checkout'}
          </button>
        </div>
      </form>
    </div>
  );
};
