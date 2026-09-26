import { Component, computed, inject, input, signal } from '@angular/core';
import { AppConfigService } from '../../core/app-config.service';

const PALETTES: Array<readonly [string, string]> = [
  ['#059669', '#065f46'],
  ['#0d9488', '#115e59'],
  ['#ea580c', '#9a3412'],
  ['#4f46e5', '#312e81'],
  ['#7c3aed', '#4c1d95'],
  ['#db2777', '#831843'],
  ['#0284c7', '#0c4a6e'],
];

const ICONS: Array<readonly [RegExp, string]> = [
  [/widget|cog|gear|tool/i, 'pi pi-cog'],
  [/gadget|phone|mobile|electro|device/i, 'pi pi-mobile'],
  [/book|read|novel/i, 'pi pi-book'],
  [/shirt|cloth|wear|fashion|shoe/i, 'pi pi-tag'],
  [/food|snack|eat|coffee|tea/i, 'pi pi-shopping-basket'],
  [/toy|game|play|sport|fit/i, 'pi pi-trophy'],
  [/beauty|health|care|soap/i, 'pi pi-heart'],
  [/home|furnit|decor|lamp|chair/i, 'pi pi-home'],
];

function hash(text: string): number {
  let value = 0;
  for (const char of text) value = (value * 31 + char.codePointAt(0)!) | 0;
  return Math.abs(value);
}

@Component({
  selector: 'app-product-art',
  imports: [],
  templateUrl: './product-art.html',
  styleUrl: './product-art.scss',
})
export class ProductArt {
  readonly name = input.required<string>();
  readonly category = input('');
  readonly compact = input(false);
  readonly imageUrl = input<string | null | undefined>(null);

  private readonly config = inject(AppConfigService);
  protected readonly imageFailed = signal(false);

  protected readonly fullImageUrl = computed(() => {
    const url = this.imageUrl();
    if (!url) return null;
    return url.startsWith('http') ? url : `${this.config.apiUrl}${url}`;
  });

  protected onImageError(): void {
    this.imageFailed.set(true);
  }

  protected readonly palette = computed(
    () => PALETTES[hash(this.category() || this.name()) % PALETTES.length],
  );

  protected readonly icon = computed(() => {
    const text = `${this.category()} ${this.name()}`;
    return ICONS.find(([pattern]) => pattern.test(text))?.[1] ?? 'pi pi-box';
  });

  protected readonly initials = computed(() => {
    const letters = this.name()
      .split(/\s+/)
      .slice(0, 2)
      .map((word) => word.charAt(0).toUpperCase())
      .join('');
    return letters || 'OF';
  });

  protected readonly gradient = computed(
    () => `linear-gradient(135deg, ${this.palette()[0]}, ${this.palette()[1]})`,
  );
}
