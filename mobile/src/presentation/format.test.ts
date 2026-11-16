import { formatMoney, statusLabel } from './format';

describe('formatMoney', () => {
  it('formats BRL amounts in Brazilian currency style', () => {
    // Intl inserts a non-breaking space after "R$", so match loosely on whitespace rather than
    // pinning the exact character.
    expect(formatMoney(1234.5)).toMatch(/^R\$\s1\.234,50$/);
  });

  it('formats another currency by its own symbol', () => {
    expect(formatMoney(10, 'USD')).toContain('10,00');
  });
});

describe('statusLabel', () => {
  it('translates every known order status', () => {
    expect(statusLabel('Pending')).toBe('Confirmando pedido');
    expect(statusLabel('AwaitingPayment')).toBe('Processando pagamento');
    expect(statusLabel('Paid')).toBe('Pago');
    expect(statusLabel('Cancelled')).toBe('Cancelado');
  });

  it('falls back to the raw value for an unknown status', () => {
    expect(statusLabel('Something')).toBe('Something');
  });
});
