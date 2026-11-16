import { createContext, ReactNode, useContext } from 'react';
import { catalogApi } from '../data/catalogApi';
import { orderingApi } from '../data/orderingApi';
import { CatalogGateway, OrderingGateway } from '../domain/ports';

type Api = { catalog: CatalogGateway; ordering: OrderingGateway };

const defaultApi: Api = { catalog: catalogApi, ordering: orderingApi };

const ApiContext = createContext<Api>(defaultApi);

/** Lets tests (or a future staging build) swap in fake gateways without touching any screen. */
export function ApiProvider({ api = defaultApi, children }: { api?: Api; children: ReactNode }) {
  return <ApiContext.Provider value={api}>{children}</ApiContext.Provider>;
}

export function useApi(): Api {
  return useContext(ApiContext);
}
