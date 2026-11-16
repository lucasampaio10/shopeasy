import { createContext, ReactNode, useCallback, useContext, useMemo, useState } from 'react';
import { addProduct, cartCurrency, cartItemCount, cartTotal, emptyCart, removeProduct, updateQuantity } from '../domain/cart';
import { Cart, Product } from '../domain/entities';

type CartContextValue = {
  cart: Cart;
  itemCount: number;
  total: number;
  currency: string;
  add(product: Product, quantity?: number): void;
  setQuantity(productId: string, quantity: number): void;
  remove(productId: string): void;
  clear(): void;
};

const CartContext = createContext<CartContextValue | null>(null);

export function CartProvider({ children }: { children: ReactNode }) {
  const [cart, setCart] = useState<Cart>(emptyCart());

  const add = useCallback((product: Product, quantity = 1) => setCart((current) => addProduct(current, product, quantity)), []);
  const setQuantity = useCallback(
    (productId: string, quantity: number) => setCart((current) => updateQuantity(current, productId, quantity)),
    [],
  );
  const remove = useCallback((productId: string) => setCart((current) => removeProduct(current, productId)), []);
  const clear = useCallback(() => setCart(emptyCart()), []);

  const value = useMemo<CartContextValue>(
    () => ({ cart, itemCount: cartItemCount(cart), total: cartTotal(cart), currency: cartCurrency(cart), add, setQuantity, remove, clear }),
    [cart, add, setQuantity, remove, clear],
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart(): CartContextValue {
  const context = useContext(CartContext);
  if (!context) {
    throw new Error('useCart precisa estar dentro de um CartProvider.');
  }

  return context;
}
