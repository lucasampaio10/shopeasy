const currencyFormatter = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

export function formatMoney(amount: number, currency = 'BRL'): string {
  if (currency !== 'BRL') {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency }).format(amount);
  }

  return currencyFormatter.format(amount);
}

const STATUS_LABELS: Record<string, string> = {
  Pending: 'Confirmando pedido',
  AwaitingPayment: 'Processando pagamento',
  Paid: 'Pago',
  Cancelled: 'Cancelado',
};

export function statusLabel(status: string): string {
  return STATUS_LABELS[status] ?? status;
}
