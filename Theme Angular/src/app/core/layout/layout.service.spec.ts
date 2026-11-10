import { TestBed } from '@angular/core/testing';
import { LayoutService } from './layout.service';

describe('LayoutService', () => {
  let service: LayoutService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(LayoutService);
  });

  it('should initialize with light theme and full sidebar defaults', () => {
    expect(service.themeMode()).toBe('light');
    expect(service.colorPreset()).toBe('cyan');
    expect(service.sidebarMode()).toBe('full');
    expect(service.cardStyle()).toBe('shadow');
  });

  it('should toggle dark theme and update html class', () => {
    service.toggleThemeMode();
    TestBed.flushEffects();
    expect(service.isDark()).toBe(true);
    expect(document.documentElement.classList.contains('dark-theme')).toBe(true);

    service.toggleThemeMode();
    TestBed.flushEffects();
    expect(service.isDark()).toBe(false);
    expect(document.documentElement.classList.contains('dark-theme')).toBe(false);
  });

  it('should switch sidebar mode and color preset', () => {
    service.setSidebarMode('mini');
    service.setColorPreset('purple');
    TestBed.flushEffects();

    expect(service.sidebarMode()).toBe('mini');
    expect(service.colorPreset()).toBe('purple');
    expect(document.documentElement.classList.contains('theme-purple')).toBe(true);
  });
});
