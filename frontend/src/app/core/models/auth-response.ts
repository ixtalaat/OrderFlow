export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  roles: string[];
  refreshToken?: string | null;
  refreshExpiresAtUtc?: string | null;
}
