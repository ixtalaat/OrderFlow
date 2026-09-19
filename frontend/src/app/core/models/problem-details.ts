export interface ProblemDetails {
  title?: string;
  status?: number;
  detail?: string;
  extensions?: {
    errors?: string[];
    traceId?: string;
  };
}
