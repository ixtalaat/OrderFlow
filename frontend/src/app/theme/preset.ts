import { definePreset, palette } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

// OrderFlow design system (design-system/orderflow/MASTER.md):
// emerald primary, orange CTA accent, Rubik headings, Nunito Sans body.
export const OrderFlowPreset = definePreset(Aura, {
  semantic: {
    primary: palette('#059669'),
    colorScheme: {
      light: {
        surface: {
          0: '#ffffff',
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          950: '#020617',
        },
      },
    },
  },
});
