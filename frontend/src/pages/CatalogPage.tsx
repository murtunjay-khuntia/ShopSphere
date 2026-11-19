import React, { useEffect, useState } from 'react';
import { Search, PlusCircle } from 'lucide-react';
import api from '../api/axiosClient';
import type { Product } from '../types';
import { useCart } from '../context/CartContext';

export const CatalogPage: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');
  const [loading, setLoading] = useState(true);
  const { addToCart } = useCart();

  const fetchProducts = async () => {
    setLoading(true);
    try {
      const { data } = await api.get<Product[]>('/products', {
        params: { search: search || undefined, category: category || undefined },
      });
      setProducts(data);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchProducts();
  }, [category]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    fetchProducts();
  };

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-8">
        <div>
          <h1 className="text-3xl font-bold text-slate-900">Product Catalog</h1>
          <p className="text-slate-500 text-sm">Real-time inventory protected by optimistic concurrency</p>
        </div>

        <form onSubmit={handleSearch} className="flex flex-wrap gap-2">
          <select
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            className="border border-slate-300 rounded-lg px-3 py-2 bg-white text-sm"
          >
            <option value="">All Categories</option>
            <option value="Electronics">Electronics</option>
            <option value="Audio">Audio</option>
            <option value="Accessories">Accessories</option>
            <option value="Lifestyle">Lifestyle</option>
          </select>

          <div className="relative">
            <input
              type="text"
              placeholder="Search products..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="border border-slate-300 rounded-lg pl-3 pr-9 py-2 text-sm bg-white"
            />
            <button type="submit" className="absolute right-2.5 top-2.5 text-slate-400 hover:text-indigo-600 cursor-pointer">
              <Search className="w-4 h-4" />
            </button>
          </div>
        </form>
      </div>

      {loading ? (
        <p className="text-center text-slate-500 py-12">Loading products...</p>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {products.map((product) => (
            <div key={product.id} className="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col">
              <img
                src={product.imageUrl || 'https://unsplash.com'}
                alt={product.name}
                className="h-48 w-full object-cover bg-slate-100"
              />
              <div className="p-5 flex-1 flex flex-col justify-between">
                <div>
                  <div className="flex items-center justify-between text-xs text-slate-500 mb-1">
                    <span className="bg-indigo-50 text-indigo-700 px-2 py-0.5 rounded font-medium">
                      {product.category}
                    </span>
                    <span>SKU: {product.sku}</span>
                  </div>
                  <h3 className="text-lg font-semibold text-slate-800">{product.name}</h3>
                  <p className="text-sm text-slate-500 mt-1 line-clamp-2">{product.description}</p>
                </div>

                <div className="mt-4 pt-4 border-t border-slate-100 flex items-center justify-between">
                  <div>
                    <span className="text-xl font-bold text-slate-900">${product.price.toFixed(2)}</span>
                    <p className={`text-xs ${product.stockQuantity > 0 ? 'text-emerald-600' : 'text-rose-600'}`}>
                      {product.stockQuantity > 0 ? `${product.stockQuantity} in stock` : 'Out of stock'}
                    </p>
                  </div>

                  <button
                    disabled={product.stockQuantity === 0}
                    onClick={() => addToCart(product)}
                    className="flex items-center gap-1.5 bg-indigo-600 hover:bg-indigo-700 disabled:bg-slate-300 text-white px-3.5 py-2 rounded-lg text-sm font-medium transition cursor-pointer"
                  >
                    <PlusCircle className="w-4 h-4" />
                    <span>Add</span>
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
