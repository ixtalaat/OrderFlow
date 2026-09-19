import { TestBed } from '@angular/core/testing';
import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('app-dark');
    TestBed.configureTestingModule({});
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.classList.remove('app-dark');
  });

  it('should toggle dark mode and persist the choice', () => {
    const service = TestBed.inject(ThemeService);
    const initial = service.dark();

    service.toggle();

    expect(service.dark()).toBe(!initial);
    expect(document.documentElement.classList.contains('app-dark')).toBe(!initial);
    expect(localStorage.getItem('orderflow_theme')).toBe(!initial ? 'dark' : 'light');
  });
});
