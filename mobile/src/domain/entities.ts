export type Money = {
  amount: number;
  currency: string;
};

export type Product = {
  id: string;
  name: string;
  sku: string;
  description: string;
  price: Money;
  availableStock: number;
};

export type CartLine = {
  productId: string;
  name: string;
  unitPrice: Money;
  quantity: number;
};

export type Cart = {
  lines: CartLine[];
};

export type ShippingAddress = {
  street: string;
  city: string;
  state: string;
  zipCode: string;
};

/** Mirrors Ordering's state machine: Pending → AwaitingPayment → Paid, or Cancelled from either. */
export type OrderStatus = 'Pending' | 'AwaitingPayment' | 'Paid' | 'Cancelled';

export type OrderLine = {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
};

export type Order = {
  id: string;
  customerId: string;
  status: OrderStatus;
  total: number;
  currency: string;
  placedAtUtc: string;
  paymentId: string | null;
  cancellationReason: string | null;
  lines: OrderLine[];
};
