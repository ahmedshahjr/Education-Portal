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

  it('starts with the light theme and full sidebar', () => {
    expect(service.themeMode()).toBe('light');
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

  it('stores the sidebar mode with the user preferences', () => {
    service.setSidebarMode('mini');
    TestBed.tick();

    expect(localStorage.getItem('education_portal_layout_prefs_v1') ?? '').toContain('mini');
  });
});
