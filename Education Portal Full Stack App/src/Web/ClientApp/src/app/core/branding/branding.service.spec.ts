import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Title } from '@angular/platform-browser';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { BrandingDto } from '../../web-api-client';
import { BrandingService } from './branding.service';
import { contrastRatio } from './sidebar-palette';

const branding = (accentColor: string, sidebarColor: string | null = null) =>
  ({ schoolName: 'Demo School', isMaster: false, accentColor, sidebarColor, logoVersion: undefined, faviconVersion: undefined }) as unknown as BrandingDto;

describe('BrandingService', () => {
  let service: BrandingService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(BrandingService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    document.documentElement.removeAttribute('style');
    http.verify();
  });

  it('applies the school name, accent colour, sidebar colour and favicon', async () => {
    service.load().subscribe();
    // The generated client reads every response as a Blob, as the browser would deliver it.
    const body = {
      schoolName: 'Demo School',
      isMaster: false,
      accentColor: '#763ebd',
      sidebarColor: '#1f2a3d',
      logoVersion: 'abc',
      faviconVersion: 'def',
    };
    http.expectOne('/api/Branding').flush(new Blob([JSON.stringify(body)], { type: 'application/json' }));
    // The Blob is read asynchronously.
    await vi.waitFor(() => expect(service.schoolName()).toBe('Demo School'));
    TestBed.tick();

    const html = document.documentElement;
    expect(service.schoolName()).toBe('Demo School');
    expect(TestBed.inject(Title).getTitle()).toBe('Demo School');
    expect(html.style.getPropertyValue('--mm-primary')).toBe('#763ebd');
    expect(html.style.getPropertyValue('--mm-sidebar-bg')).toBe('#1f2a3d');
    expect(html.style.getPropertyValue('--mm-sidebar-text')).toBe('#ffffff');
    expect(service.logoUrl()).toBe('api/Branding/images/Logo?v=abc');
    expect(document.querySelector<HTMLLinkElement>('link[rel="icon"]')?.getAttribute('href')).toBe('api/Branding/images/Favicon?v=def');
  });

  it('falls back to the theme defaults when the school has no colour or images', () => {
    service.apply(new BrandingDto({ schoolName: 'Plain School', isMaster: false, accentColor: '#00a1ff', sidebarColor: undefined, logoVersion: undefined, faviconVersion: undefined }));

    expect(document.documentElement.style.getPropertyValue('--mm-sidebar-bg')).toBe('');
    expect(service.logoUrl()).toBeNull();
    expect(document.querySelector<HTMLLinkElement>('link[rel="icon"]')?.getAttribute('href')).toBe('favicon.png');
  });

  it('keeps the defaults when the branding cannot be loaded', () => {
    let completed = false;
    service.load().subscribe({ complete: () => (completed = true) });
    http.expectOne('/api/Branding').flush(null, { status: 500, statusText: 'Server Error' });

    expect(completed).toBe(true);
    expect(service.schoolName()).toBe('Education Portal');
  });

  it('derives the accent palette from a #rrggbb colour, and clears it for anything else', () => {
    service.apply(branding('#00A1FF'));
    const style = document.documentElement.style;
    expect(style.getPropertyValue('--mm-primary')).toBe('#00a1ff');
    expect(style.getPropertyValue('--mm-primary-rgb')).toBe('0, 161, 255');
    expect(style.getPropertyValue('--mm-primary-hover')).toBe('#008ee0');

    service.apply(branding('orange'));
    expect(style.getPropertyValue('--mm-primary')).toBe('');
    service.apply(branding('#123456; background: url(x)'));
    expect(style.getPropertyValue('--mm-primary')).toBe('');
  });

  it('uses light text on a dark sidebar and dark text on a light one, readable (4.5:1) for every colour', () => {
    const style = document.documentElement.style;

    service.apply(branding('#763ebd', '#1f2a3d'));
    expect(style.getPropertyValue('--mm-sidebar-text')).toBe('#ffffff');
    service.apply(branding('#763ebd', '#f5f5dc'));
    expect(style.getPropertyValue('--mm-sidebar-text')).toBe('#111c2d');

    for (const colour of ['#000000', '#ffffff', '#ff0000', '#00a1ff', '#777777', '#ffae1f']) {
      service.apply(branding('#763ebd', colour));
      const text = style.getPropertyValue('--mm-sidebar-text');
      expect(contrastRatio(colour, text)).toBeGreaterThanOrEqual(4.5);
    }
  });

  it('removes the sidebar colours for a sidebar colour that is not #rrggbb', () => {
    const style = document.documentElement.style;
    service.apply(branding('#763ebd', '#1f2a3d'));
    service.apply(branding('#763ebd', 'red'));
    expect(style.getPropertyValue('--mm-sidebar-text')).toBe('');
  });
});
