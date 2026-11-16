import { Order } from '../domain/entities';
import { OrderingGateway } from '../domain/ports';
import { httpClient } from './httpClient';

// The response shape already matches our `Order` type field-for-field (OrderResponse on the API).
export const orderingApi: OrderingGateway = {
  placeOrder: (input) => httpClient.post<{ id: string }>('/api/orders', input),
  getOrder: (id) => httpClient.get<Order>(`/api/orders/${id}`),
};
