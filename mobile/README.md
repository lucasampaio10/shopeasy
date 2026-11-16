# ShopEasy — app

Loja em **React Native (Expo)** para o backend de microsserviços do ShopEasy: catálogo, carrinho, checkout com dois métodos de pagamento simulados e acompanhamento do pedido em tempo real, direto pela saga do backend.

## Arquitetura

O app segue a mesma ideia de Clean Architecture do backend, só que em camadas de front-end:

```
src/
├── domain/         # Entidades e regras puras: cart.ts, checkout.ts — sem React, sem rede
├── data/           # Implementações HTTP dos ports (catalogApi, orderingApi) + config
├── application/     # Casos de uso: hooks (useProducts, usePlaceOrder, useOrderTracking) e contexto do carrinho
└── presentation/    # Telas, componentes e navegação
```

As telas e os hooks nunca chamam `fetch` diretamente — dependem das interfaces em `domain/ports.ts` (`CatalogGateway`, `OrderingGateway`). Isso é inversão de dependência de propósito: os testes de `usePlaceOrder` (removidos nesta versão por incompatibilidade de ferramentas — ver nota abaixo) injetavam um gateway falso sem precisar de rede nem mockar `fetch`.

## Telas

`Products` → `ProductDetail` → `Cart` → `Checkout` → `OrderStatus`

Em `Checkout`, o método de pagamento é escolhido entre dois tokens que o gateway simulado do backend reconhece — útil para mostrar as duas pontas da saga do pedido sem precisar de um cartão de verdade:

| Opção na tela | Token enviado | Resultado |
|---|---|---|
| Cartão aprovado | `tok_visa` | Pedido pago |
| Cartão recusado | `tok_declined_card` | Pedido cancelado, estoque devolvido |

`OrderStatus` consulta `GET /api/orders/{id}` a cada 1,5s até o pedido chegar a `Paid` ou `Cancelled` — o mesmo padrão de *polling* que o painel do DataViz Dashboard usa para acompanhar sua projeção.

## Como rodar

Pré-requisitos: Node 20+, e o backend do ShopEasy no ar (`docker compose up -d` na raiz do repositório).

```bash
npm install
```

```bash
npm run web
```

Isso abre o app no navegador, falando com `http://localhost:8080` (o gateway) por padrão. Para rodar no celular físico ou no simulador, copie `.env.example` para `.env` e ajuste `EXPO_PUBLIC_API_URL` para o IP da máquina na rede:

```bash
npm run android   # ou npm run ios
```

### Build de produção (web)

```bash
EXPO_PUBLIC_API_URL=http://localhost:8080 npx expo export --platform web
```

Gera uma pasta `dist/` estática, servida pelo `Dockerfile` deste diretório (`docker compose up -d storefront` na raiz sobe isso em `http://localhost:8090`).

## Testes

```bash
npm test
```

Cobrem as regras puras do carrinho (`cart.ts`) e da validação de endereço (`checkout.ts`), além da formatação de moeda e dos rótulos de status. Não há testes de hooks/componentes nesta versão: o Expo SDK 58 (React 19.3 + React Native 0.88) é recente o bastante para que o `@testing-library/react-native` 14.1.0 ainda não renderize hooks de forma estável nesse combo — os testes de `useCart`/`usePlaceOrder` que dependiam disso foram removidos em vez de mantidos quebrados. Vale revisitar quando o ecossistema estabilizar.

## Fora do escopo

Autenticação (o id do cliente é gerado por sessão, sem login), persistência do carrinho entre sessões, e build nativo para as lojas de aplicativos.
