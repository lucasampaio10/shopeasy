import { useState } from 'react';
import { ApiError } from '../data/httpClient';
import { PlaceOrderInput } from '../domain/ports';
import { useApi } from './ApiContext';

export function usePlaceOrder() {
  const { ordering } = useApi();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function placeOrder(input: PlaceOrderInput): Promise<string> {
    setSubmitting(true);
    setError(null);

    try {
      const { id } = await ordering.placeOrder(input);
      return id;
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : 'Não foi possível enviar o pedido.');
      throw cause;
    } finally {
      setSubmitting(false);
    }
  }

  return { placeOrder, submitting, error };
}
