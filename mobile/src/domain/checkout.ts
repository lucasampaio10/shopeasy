import { ShippingAddress } from './entities';

/**
 * Payment tokens the backend's simulated gateway recognises. The real app would hand a card
 * form to a payment SDK and receive a token back; here the token itself picks the outcome, which
 * is deliberately exposed as a choice on the Checkout screen so both branches of the order saga
 * (approved and declined) are easy to see.
 */
export const PAYMENT_METHODS = [
  { token: 'tok_visa', label: 'Cartão aprovado', hint: 'Visa •••• 4242 — passa na simulação' },
  { token: 'tok_declined_card', label: 'Cartão recusado', hint: 'Para testar o fluxo de recusa' },
] as const;

export type PaymentToken = (typeof PAYMENT_METHODS)[number]['token'];

export function validateShippingAddress(address: ShippingAddress): string[] {
  const errors: string[] = [];
  if (!address.street.trim()) errors.push('Informe o endereço.');
  if (!address.city.trim()) errors.push('Informe a cidade.');
  if (!address.state.trim()) errors.push('Informe o estado.');
  if (!address.zipCode.trim()) errors.push('Informe o CEP.');

  return errors;
}
