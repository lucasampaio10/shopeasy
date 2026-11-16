import { Product } from '../domain/entities';
import { CatalogGateway } from '../domain/ports';
import { httpClient } from './httpClient';

type ProductDto = {
  id: string;
  name: string;
  sku: string;
  description: string;
  price: number;
  currency: string;
  availableStock: number;
};

type PagedResponseDto<T> = { items: T[]; page: number; pageSize: number; totalItems: number };

const PAGE_SIZE = 50;

function toProduct(dto: ProductDto): Product {
  return {
    id: dto.id,
    name: dto.name,
    sku: dto.sku,
    description: dto.description,
    price: { amount: dto.price, currency: dto.currency },
    availableStock: dto.availableStock,
  };
}

export const catalogApi: CatalogGateway = {
  async listProducts(search) {
    const params = new URLSearchParams({ pageSize: String(PAGE_SIZE) });
    if (search) {
      params.set('search', search);
    }

    const page = await httpClient.get<PagedResponseDto<ProductDto>>(`/api/products?${params}`);
    return page.items.map(toProduct);
  },

  async getProduct(id) {
    const dto = await httpClient.get<ProductDto>(`/api/products/${id}`);
    return toProduct(dto);
  },
};
