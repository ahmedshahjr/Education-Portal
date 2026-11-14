import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Title } from '@angular/platform-browser';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { BrandingDto } from '../../web-api-client';
import { BrandingService } from './branding.service';

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
});
