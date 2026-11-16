import { Order, Product, ShippingAddress } from './entities';

/**
 * Ports the application layer depends on. Screens and hooks only ever see these interfaces —
 * never `fetch` directly — so the real HTTP clients in `data/` can be swapped for fakes in tests
 * without touching a single component (dependency inversion).
 */
export interface CatalogGateway {
  listProducts(search?: string): Promise<Product[]>;
  getProduct(id: string): Promise<Product>;
}

export type PlaceOrderInput = {
  customerId: string;
  shippingAddress: ShippingAddress;
  paymentToken: string;
  items: { productId: string; quantity: number }[];
};

export interface OrderingGateway {
  placeOrder(input: PlaceOrderInput): Promise<{ id: string }>;
  getOrder(id: string): Promise<Order>;
}
