export interface Product {
  id: number;
  name: string;
  description: string | null;
  price: number;
  stock: number;
  imageUrl: string | null;
  categoryId: number;
  categoryName: string;
}

export interface CartItem {
  productId: number;
  productName: string;
  unitPrice: number;
  quantity: number;
  subtotal: number;
}

export interface Cart {
  cartId: number;
  items: CartItem[];
  total: number;
}

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface Order {
  id: number;
  status: string;
  totalAmount: number;
  createdAt: string;
  items: OrderItem[];
}

export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
  role: string;
}
