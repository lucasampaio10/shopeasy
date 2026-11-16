import { Cart, Product } from './entities';

/**
 * Pure cart rules — no React, no network, nothing mutated in place. Kept separate from any
 * screen so they can be unit-tested on their own and reused if the cart ever needs a different
 * UI (e.g. a "buy again" shortcut).
 */
export function emptyCart(): Cart {
  return { lines: [] };
}

export function addProduct(cart: Cart, product: Product, quantity = 1): Cart {
  if (quantity <= 0) {
    throw new Error('A quantidade precisa ser maior que zero.');
  }

  const existing = cart.lines.find((line) => line.productId === product.id);
  if (existing) {
    return {
      lines: cart.lines.map((line) =>
        line.productId === product.id ? { ...line, quantity: line.quantity + quantity } : line,
      ),
    };
  }

  return {
    lines: [
      ...cart.lines,
      { productId: product.id, name: product.name, unitPrice: product.price, quantity },
    ],
  };
}

export function updateQuantity(cart: Cart, productId: string, quantity: number): Cart {
  if (quantity <= 0) {
    return removeProduct(cart, productId);
  }

  return { lines: cart.lines.map((line) => (line.productId === productId ? { ...line, quantity } : line)) };
}

export function removeProduct(cart: Cart, productId: string): Cart {
  return { lines: cart.lines.filter((line) => line.productId !== productId) };
}

export function cartItemCount(cart: Cart): number {
  return cart.lines.reduce((sum, line) => sum + line.quantity, 0);
}

export function cartTotal(cart: Cart): number {
  return cart.lines.reduce((sum, line) => sum + line.unitPrice.amount * line.quantity, 0);
}

export function cartCurrency(cart: Cart): string {
  return cart.lines[0]?.unitPrice.currency ?? 'BRL';
}

export function isCartEmpty(cart: Cart): boolean {
  return cart.lines.length === 0;
}
