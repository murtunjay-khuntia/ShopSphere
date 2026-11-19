import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ShoppingCart, Package, ShieldCheck, LogOut, Store } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';

export const Navbar: React.FC = () => {
  const { user, logout, isAdmin } = useAuth();
  const { totalCount } = useCart();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <nav className="bg-slate-900 text-white shadow-md sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 py-3 flex items-center justify-between">
        <Link to="/" className="flex items-center gap-2 text-xl font-bold text-indigo-400">
          <Store className="w-6 h-6" />
          <span>ShopSphere</span>
        </Link>

        <div className="flex items-center gap-5 text-sm font-medium">
          <Link to="/" className="hover:text-indigo-300 transition">Catalog</Link>

          <Link to="/cart" className="relative flex items-center gap-1 hover:text-indigo-300 transition">
            <ShoppingCart className="w-5 h-5" />
            <span>Cart</span>
            {totalCount > 0 && (
              <span className="bg-indigo-500 text-white text-xs rounded-full px-2 py-0.5">
                {totalCount}
              </span>
            )}
          </Link>

          {user && (
            <Link to="/orders" className="flex items-center gap-1 hover:text-indigo-300 transition">
              <Package className="w-4 h-4" />
              <span>My Orders</span>
            </Link>
          )}

          {isAdmin && (
            <Link to="/admin" className="flex items-center gap-1 text-amber-400 hover:text-amber-300 transition">
              <ShieldCheck className="w-4 h-4" />
              <span>Admin Dashboard</span>
            </Link>
          )}

          {user ? (
            <div className="flex items-center gap-3 pl-3 border-l border-slate-700">
              <span className="text-slate-300 hidden sm:inline">
                {user.fullName} ({user.role})
              </span>
              <button
                onClick={handleLogout}
                className="flex items-center gap-1 bg-rose-600 hover:bg-rose-700 px-3 py-1.5 rounded-md transition cursor-pointer"
              >
                <LogOut className="w-4 h-4" />
                <span>Logout</span>
              </button>
            </div>
          ) : (
            <Link
              to="/login"
              className="bg-indigo-600 hover:bg-indigo-700 px-4 py-1.5 rounded-md transition"
            >
              Sign In
            </Link>
          )}
        </div>
      </div>
    </nav>
  );
};
