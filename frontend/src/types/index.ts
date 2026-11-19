export interface AuthResponse {
  userId: string;
  fullName: string;
  email: string;
  role: 'Customer' | 'Admin';
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
}

export interface Product {
  id: string;
  name: string;
  description: string;
  category: string;
  sku: string;
  price: number;
  stockQuantity: number;
  imageUrl?: string;
  isActive: boolean;
}

export interface CartItem {
  product: Product;
  quantity: number;
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subTotal: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  customerName: string;
  customerEmail: string;
  status: string;
  totalAmount: number;
  shippingAddress: string;
  createdAt: string;
  items: OrderItem[];
}

export interface SalesSummary {
  totalOrders: number;
  completedOrders: number;
  cancelledOrders: number;
  totalRevenue: number;
  averageOrderValue: number;
}

export interface TopProductReport {
  productId: string;
  productName: string;
  category: string;
  totalUnitsSold: number;
  totalRevenue: number;
}
