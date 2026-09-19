import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { MessageService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';
import { OrderFlowPreset } from './theme/preset';

import { routes } from './app.routes';
import { AppConfigService } from './core/app-config.service';
import { AuthStore } from './core/auth-store';
import { authInterceptor } from './core/auth-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideAnimationsAsync(),
    // PrimeNG v20 resolves dark mode via the CSS color-scheme property
    // (light-dark()), so the .app-dark class only needs color-scheme: dark.
    providePrimeNG({
      theme: { preset: OrderFlowPreset },
    }),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAppInitializer(() => {
      const config = inject(AppConfigService);
      const auth = inject(AuthStore);
      return (async () => {
        await config.load();
        await auth.restoreSession();
      })();
    }),
    MessageService,
  ],
};
