export interface ManagedCustomer {
  id: number;
  userId: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  address: string;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string | null;
}
