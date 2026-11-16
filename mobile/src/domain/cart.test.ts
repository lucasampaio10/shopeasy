import { addProduct, cartCurrency, cartItemCount, cartTotal, emptyCart, isCartEmpty, removeProduct, updateQuantity } from './cart';
import { Product } from './entities';

function product(overrides: Partial<Product> = {}): Product {
  return {
    id: 'p1',
    name: 'Teclado Mecânico',
    sku: 'KEY-K8',
    description: 'Switches táteis',
    price: { amount: 100, currency: 'BRL' },
    availableStock: 10,
    ...overrides,
  };
}

describe('cart', () => {
  it('starts empty', () => {
    expect(isCartEmpty(emptyCart())).toBe(true);
  });

  it('adds a new product as its own line', () => {
    const cart = addProduct(emptyCart(), product(), 2);

    expect(cart.lines).toEqual([{ productId: 'p1', name: 'Teclado Mecânico', unitPrice: { amount: 100, currency: 'BRL' }, quantity: 2 }]);
  });

  it('adding the same product again increases the existing line instead of duplicating it', () => {
    const cart = addProduct(addProduct(emptyCart(), product(), 1), product(), 2);

    expect(cart.lines).toHaveLength(1);
    expect(cart.lines[0].quantity).toBe(3);
  });

  it('rejects a non-positive quantity', () => {
    expect(() => addProduct(emptyCart(), product(), 0)).toThrow();
  });

  it('updates the quantity of an existing line', () => {
    const cart = updateQuantity(addProduct(emptyCart(), product()), 'p1', 5);

    expect(cart.lines[0].quantity).toBe(5);
  });

  it('dropping the quantity to zero removes the line, same as removeProduct', () => {
    const cart = updateQuantity(addProduct(emptyCart(), product()), 'p1', 0);

    expect(isCartEmpty(cart)).toBe(true);
  });

  it('removes a line by product id, leaving the others untouched', () => {
    const cart = addProduct(addProduct(emptyCart(), product({ id: 'p1' })), product({ id: 'p2' }));

    const result = removeProduct(cart, 'p1');

    expect(result.lines.map((line) => line.productId)).toEqual(['p2']);
  });

  it('counts total items across every line', () => {
    const cart = addProduct(addProduct(emptyCart(), product({ id: 'p1' }), 2), product({ id: 'p2' }), 3);

    expect(cartItemCount(cart)).toBe(5);
  });

  it('totals the cart as price × quantity, summed', () => {
    const cart = addProduct(
      addProduct(emptyCart(), product({ id: 'p1', price: { amount: 100, currency: 'BRL' } }), 2),
      product({ id: 'p2', price: { amount: 30, currency: 'BRL' } }),
      1,
    );

    expect(cartTotal(cart)).toBe(230);
  });

  it('reads the currency from the cart, defaulting to BRL when empty', () => {
    expect(cartCurrency(emptyCart())).toBe('BRL');
    expect(cartCurrency(addProduct(emptyCart(), product({ price: { amount: 10, currency: 'USD' } })))).toBe('USD');
  });
});
