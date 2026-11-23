import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { describe, expect, it, vi } from 'vitest';
import { AuthService } from '../../../core/auth/auth.service';
import { BrandingService } from '../../../core/branding/branding.service';
import { APP_NAV_SECTIONS } from '../../../core/layout/navigation';
import { API_BASE_URL, BrandingDto } from '../../../web-api-client';
import { SidebarComponent } from './sidebar.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const tick = () => new Promise((resolve) => setTimeout(resolve, 0));
/** Waits for the request to be made: each response is read as a Blob, so the next request comes a few ticks later. */
async function answer(http: HttpTestingController, match: (url: string) => boolean, body: unknown) {
  await vi.waitFor(() => http.expectOne((r) => match(r.url)).flush(json(body)));
  for (let i = 0; i < 5; i++) {
    await tick();
  }
}

/** The routes of the items shown in the sidebar, read from the menu definition the app uses. */
const routeOf = (id: string) => APP_NAV_SECTIONS.flatMap((s) => s.items).find((i) => i.id === id)!.route;

/**
 * Signs in through the real AuthService, then loads the real modules and rights, answering each request over
 * HttpTestingController, and renders the real sidebar.
 */
async function renderSidebar(options: { administrator: boolean; master: boolean; modules: string[]; rights?: { module: string; rights: { right: string; campusIds?: string[] }[] }[] }) {
  TestBed.configureTestingModule({
    imports: [SidebarComponent],
    providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const http = TestBed.inject(HttpTestingController);
  const auth = TestBed.inject(AuthService);
  TestBed.inject(BrandingService).apply({ schoolName: 'Demo', isMaster: options.master, accentColor: '#763ebd' } as unknown as BrandingDto);

  auth.initialize().subscribe();
  await answer(http, (u) => u === '/api/Users/me', { userName: 'admin@demo.localhost', roles: options.administrator ? ['Administrator'] : [], mustChangePassword: false });
  await answer(http, (u) => u.startsWith('/api/Modules/enabled'), options.modules);
  await answer(http, (u) => u === '/api/Users/me/rights', { modules: options.rights ?? [], isParent: false });

  const fixture = TestBed.createComponent(SidebarComponent);
  fixture.detectChanges();
  const element = fixture.nativeElement as HTMLElement;
  const hrefs = () => [...element.querySelectorAll('a[href]')].map((a) => a.getAttribute('href'));
  return { fixture, element, hrefs };
}

describe('SidebarComponent with the real sign-in, modules and rights (integration)', () => {
  it('shows only the dashboard to a signed-in user with no role', async () => {
    const { hrefs } = await renderSidebar({ administrator: false, master: false, modules: [] });
    expect(hrefs()).toContain(routeOf('dashboard'));
    expect(hrefs()).not.toContain(routeOf('subscription'));
  });

  it('shows the subscription to a school administrator, and hides the master portal', async () => {
    const { hrefs } = await renderSidebar({ administrator: true, master: false, modules: [] });
    expect(hrefs()).toContain(routeOf('subscription'));
    expect(hrefs()).not.toContain(routeOf('schools'));
  });

  it('shows class offerings to a school administrator only when the School module is on', async () => {
    const off = await renderSidebar({ administrator: true, master: false, modules: [] });
    expect(off.hrefs()).not.toContain(routeOf('class-offerings'));
    TestBed.resetTestingModule();

    const on = await renderSidebar({ administrator: true, master: false, modules: ['School'] });
    expect(on.hrefs()).toContain(routeOf('class-offerings'));
  });

  it('shows the master portal to the master tenant administrator', async () => {
    const { hrefs } = await renderSidebar({ administrator: true, master: true, modules: [] });
    expect(hrefs()).toContain(routeOf('schools'));
    expect(hrefs()).toContain(routeOf('pricing'));
    expect(hrefs()).toContain(routeOf('invoices'));
  });

  it('hides the change history unless the user has the right to view it', async () => {
    const without = await renderSidebar({ administrator: true, master: false, modules: ['School'] });
    expect(without.hrefs()).not.toContain(routeOf('audit'));
    TestBed.resetTestingModule();

    const withRight = await renderSidebar({ administrator: true, master: false, modules: ['School'], rights: [{ module: 'School', rights: [{ right: 'ViewHistory' }] }] });
    expect(withRight.hrefs()).toContain(routeOf('audit'));
  });

});
