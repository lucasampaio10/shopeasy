import { useEffect, useState } from 'react';
import { Order, OrderStatus } from '../domain/entities';
import { useApi } from './ApiContext';

const POLL_INTERVAL_MS = 1500;
const SETTLED: readonly OrderStatus[] = ['Paid', 'Cancelled'];

/**
 * Placing an order only guarantees it was accepted — stock and payment settle afterwards on
 * their own services. This mirrors that by polling the order until it reaches a final state,
 * the same pattern the DataViz dashboard uses to stay in sync with its read model.
 */
export function useOrderTracking(orderId: string) {
  const { ordering } = useApi();
  const [order, setOrder] = useState<Order | null>(null);
  const [failedToReach, setFailedToReach] = useState(false);

  useEffect(() => {
    let cancelled = false;
    let timer: ReturnType<typeof setTimeout>;

    async function poll() {
      try {
        const current = await ordering.getOrder(orderId);
        if (cancelled) return;

        setOrder(current);
        setFailedToReach(false);
        if (!SETTLED.includes(current.status)) {
          timer = setTimeout(poll, POLL_INTERVAL_MS);
        }
      } catch {
        if (!cancelled) {
          setFailedToReach(true);
          timer = setTimeout(poll, POLL_INTERVAL_MS);
        }
      }
    }

    void poll();

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
  }, [ordering, orderId]);

  return { order, failedToReach, isSettled: order ? SETTLED.includes(order.status) : false };
}
