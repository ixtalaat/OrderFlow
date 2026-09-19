export const OrderStatusLabels: Record<number, string> = {
  0: 'Draft',
  1: 'Submitted',
  2: 'Confirmed',
  3: 'Processing',
  4: 'Completed',
  5: 'Rejected',
  6: 'Cancelled',
};

export function orderStatusLabel(status: number): string {
  return OrderStatusLabels[status] ?? `Unknown (${status})`;
}
