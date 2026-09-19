import { expect, test } from '@playwright/test';

test('theme toggle states', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Every order in balance.' })).toBeVisible();
  await page.screenshot({ path: 'test-results/theme-light.png' });
  await page.getByRole('button', { name: /mode/ }).click();
  await page.waitForTimeout(500);
  await page.screenshot({ path: 'test-results/theme-dark.png' });
});
