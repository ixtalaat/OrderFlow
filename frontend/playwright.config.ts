import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  retries: 0,
  use: {
    baseURL: process.env['E2E_BASE_URL'] ?? 'http://localhost:4200',
    screenshot: 'only-on-failure',
  },
  webServer: process.env['E2E_NO_SERVE']
    ? undefined
    : {
        command: 'npx ng serve --port 4200',
        url: 'http://localhost:4200',
        reuseExistingServer: true,
        timeout: 180000,
      },
});
