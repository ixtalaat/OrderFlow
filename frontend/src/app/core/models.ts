export interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  roles: string[];
  refreshToken?: string | null;
  refreshExpiresAtUtc?: string | null;
}

export interface RefreshResponse {
  accessToken: string;
  expiresAtUtc: string;
  refreshToken: string;
  refreshExpiresAtUtc: string;
}

export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  extensions?: {
    errors?: string[];
    traceId?: string;
  };
}
