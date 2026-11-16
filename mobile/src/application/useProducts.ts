import { useEffect, useState } from 'react';
import { Product } from '../domain/entities';
import { useApi } from './ApiContext';

export type LoadStatus = 'loading' | 'ready' | 'error';

type Result = { search: string; status: LoadStatus; products: Product[] };

export function useProducts(search: string) {
  const { catalog } = useApi();
  // Keyed by the search that produced it: setState only ever happens inside the async
  // callback, never synchronously at the top of the effect, so a change in `search` is read as
  // "loading" by comparing it against `result.search` below rather than by resetting state here.
  const [result, setResult] = useState<Result>(() => ({ search, status: 'loading', products: [] }));

  useEffect(() => {
    let cancelled = false;

    catalog.listProducts(search || undefined).then(
      (items) => {
        if (!cancelled) {
          setResult({ search, status: 'ready', products: items });
        }
      },
      () => {
        if (!cancelled) {
          setResult({ search, status: 'error', products: [] });
        }
      },
    );

    return () => {
      cancelled = true;
    };
  }, [catalog, search]);

  const isCurrent = result.search === search;

  return { products: isCurrent ? result.products : [], status: isCurrent ? result.status : 'loading' };
}
