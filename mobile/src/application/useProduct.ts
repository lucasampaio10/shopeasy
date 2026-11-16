import { useEffect, useState } from 'react';
import { Product } from '../domain/entities';
import { useApi } from './ApiContext';
import { LoadStatus } from './useProducts';

type Result = { productId: string; status: LoadStatus; product: Product | null };

export function useProduct(productId: string) {
  const { catalog } = useApi();
  const [result, setResult] = useState<Result>(() => ({ productId, status: 'loading', product: null }));

  useEffect(() => {
    let cancelled = false;

    catalog.getProduct(productId).then(
      (found) => {
        if (!cancelled) {
          setResult({ productId, status: 'ready', product: found });
        }
      },
      () => {
        if (!cancelled) {
          setResult({ productId, status: 'error', product: null });
        }
      },
    );

    return () => {
      cancelled = true;
    };
  }, [catalog, productId]);

  const isCurrent = result.productId === productId;

  return { product: isCurrent ? result.product : null, status: isCurrent ? result.status : 'loading' };
}
