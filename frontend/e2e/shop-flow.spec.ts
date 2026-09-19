import { expect, request, test } from '@playwright/test';

const apiUrl = process.env['E2E_API_URL'] ?? 'http://localhost:8080';
const mailpitUrl = process.env['E2E_MAILPIT_URL'] ?? 'http://localhost:8025';
const adminEmail = process.env['E2E_ADMIN_EMAIL'] ?? 'admin@orderflow.com';
const adminPassword = process.env['E2E_ADMIN_PASSWORD'] ?? 'Admin123!';

async function adminToken(): Promise<string> {
  const api = await request.newContext();
  const login = await api.post(`${apiUrl}/api/auth/login`, {
    data: { email: adminEmail, password: adminPassword },
  });
  expect(login.ok()).toBeTruthy();
  const body = await login.json();
  await api.dispose();
  return body.accessToken as string;
}

async function confirmationToken(email: string): Promise<string> {
  const mail = await request.newContext();
  for (let attempt = 0; attempt < 30; attempt++) {
    const messages = await mail.get(`${mailpitUrl}/api/v1/messages`);
    const body = await messages.json();
    const found = (body.messages as { To: { Address: string }[]; ID: string }[]).find((message) =>
      message.To.some((recipient) => recipient.Address === email),
    );
    if (found) {
      const full = await mail.get(`${mailpitUrl}/api/v1/message/${found.ID}`);
      const text = JSON.stringify(await full.json());
      const match = /token: ([A-Za-z0-9+/=_-]+)/.exec(text);
      if (match) {
        await mail.dispose();
        return match[1];
      }
    }
    await new Promise((resolve) => setTimeout(resolve, 1000));
  }
  await mail.dispose();
  throw new Error(`Confirmation email for ${email} never arrived.`);
}

test('customer registers, confirms, shops, orders, and cancels', async ({ page }) => {
  const token = await adminToken();
  const email = `e2e-${Date.now()}@example.com`;
  const sku = `E2E-${Date.now().toString().slice(-8)}`;

  const api = await request.newContext({
    extraHTTPHeaders: { Authorization: `Bearer ${token}` },
  });
  const product = await api.post(`${apiUrl}/api/products`, {
    data: { name: 'E2E Widget', description: 'E2E', sku, price: 25, categoryName: 'E2E' },
  });
  expect(product.ok()).toBeTruthy();
  const productId = ((await product.json()) as { id: number }).id;
  await api.post(`${apiUrl}/api/inventory/${productId}/add-stock`, { data: { quantity: 5 } });
  await api.dispose();

  await page.goto('/register');
  await page.getByLabel('Full name').fill('E2E Customer');
  await page.getByLabel('Email').fill(email);
  await page.locator('input[type="password"]').fill('Password@123');
  await page.getByRole('button', { name: 'Register' }).click();
  await expect(page).toHaveURL(/confirm-email/);

  const confirmToken = await confirmationToken(email);
  await page.getByLabel('Email').fill(email);
  await page.getByLabel('Confirmation token').fill(confirmToken);
  await page.getByRole('button', { name: 'Confirm' }).click();
  await expect(page).toHaveURL(/login/);

  await page.getByLabel('Email').fill(email);
  await page.locator('input[type="password"]').fill('Password@123');
  await page.getByRole('button', { name: 'Log in' }).click();
  await expect(page).toHaveURL(/catalog|\/$/);

  await page.goto('/catalog');
  await page.getByPlaceholder('Search name or SKU').fill(sku);
  await page.getByRole('button', { name: 'Search' }).click();
  await expect(page.getByText('E2E Widget', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Add to cart' }).first().click();

  await page.goto('/cart');
  await expect(page.getByText('Subtotal: 25', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Checkout' }).click();
  await expect(page).toHaveURL(/checkout/);
  await page.getByRole('button', { name: 'Place order' }).click();
  await expect(page).toHaveURL(/\/orders\/\d+/);

  await page.getByRole('button', { name: 'Cancel order' }).click();
  await expect(page.getByText('Cancelled', { exact: true })).toBeVisible();
});
