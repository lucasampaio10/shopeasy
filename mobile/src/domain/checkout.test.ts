import { validateShippingAddress } from './checkout';

describe('validateShippingAddress', () => {
  const complete = { street: 'Av. Beira Mar, 1000', city: 'Fortaleza', state: 'CE', zipCode: '60165-121' };

  it('accepts a fully filled address', () => {
    expect(validateShippingAddress(complete)).toEqual([]);
  });

  it('reports one error per missing field', () => {
    const errors = validateShippingAddress({ street: '', city: '', state: '', zipCode: '' });

    expect(errors).toHaveLength(4);
  });

  it('treats whitespace-only values as missing', () => {
    expect(validateShippingAddress({ ...complete, street: '   ' })).toContain('Informe o endereço.');
  });

  it('reports only the fields that are actually missing', () => {
    expect(validateShippingAddress({ ...complete, city: '' })).toEqual(['Informe a cidade.']);
  });
});
