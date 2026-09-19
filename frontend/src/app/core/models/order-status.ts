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

export type TagSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast';

export function orderStatusSeverity(status: number): TagSeverity {
  switch (status) {
    case 4:
      return 'success';
    case 5:
    case 6:
      return 'danger';
    case 1:
    case 2:
      return 'info';
    case 3:
      return 'warn';
    default:
      return 'secondary';
  }
}
