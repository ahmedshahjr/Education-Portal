import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { LayoutService } from './layout.service';

describe('LayoutService', () => {
  let service: LayoutService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(LayoutService);
  });

  it('starts with the light theme, full sidebar and the cyan preset', () => {
    expect(service.themeMode()).toBe('light');
    expect(service.colorPreset()).toBe('cyan');
    expect(service.sidebarMode()).toBe('full');
    expect(service.cardStyle()).toBe('shadow');
  });

  it('toggles the dark theme on the html element', () => {
    service.toggleThemeMode();
    TestBed.tick();
    expect(document.documentElement.classList.contains('dark-theme')).toBe(true);

    service.toggleThemeMode();
    TestBed.tick();
    expect(document.documentElement.classList.contains('dark-theme')).toBe(false);
  });

  it('applies the school preset as one theme class', () => {
    service.setColorPreset('purple');
    TestBed.tick();

    expect(document.documentElement.classList.contains('theme-purple')).toBe(true);
    expect(document.documentElement.classList.contains('theme-cyan')).toBe(false);
  });

  it('does not store the school preset with the user preferences', () => {
    service.setColorPreset('coral');
    service.setSidebarMode('mini');
    TestBed.tick();

    const stored = localStorage.getItem('education_portal_layout_prefs_v1') ?? '';
    expect(stored).toContain('mini');
    expect(stored).not.toContain('coral');
  });
});
