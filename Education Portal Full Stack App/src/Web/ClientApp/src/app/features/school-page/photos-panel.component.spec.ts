import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { PHOTO_MAX_BYTES } from './photos.logic';
import { PhotosPanelComponent } from './photos-panel.component';
import { PHOTO_KINDS, photoKindLabel } from './school-page.labels';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

async function render() {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(PhotosPanelComponent);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();
  http.match((r) => r.url.startsWith('/api/SchoolPhotos') && r.method === 'GET').forEach((r) => r.flush(blob([])));
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelector('input[type="file"]')).not.toBeNull();
  });
  return { fixture, http, element };
}

/** After a photo is saved the panel reloads the gallery; answer that request so nothing is left running when the test ends. */
async function reloadGallery(http: HttpTestingController) {
  await vi.waitFor(() => expect(http.match((r) => r.url.startsWith('/api/SchoolPhotos') && r.method === 'GET').length).toBeGreaterThan(0));
  http.match((r) => r.url.startsWith('/api/SchoolPhotos') && r.method === 'GET').forEach((r) => r.flush(blob([])));
}

/** Picks a file the way the hidden file input does, then answers nothing: the tests look at what was sent. */
function pick(fixture: { componentInstance: unknown }, file: File) {
  const input = { files: { item: () => file }, value: 'picked' } as unknown as HTMLInputElement;
  (fixture.componentInstance as unknown as { upload(input: HTMLInputElement): void }).upload(input);
}

describe('PhotosPanelComponent', () => {
  it.each(['image/png', 'image/jpeg', 'image/webp'])('sends a %s photo to the school', async (type) => {
    const { fixture, http } = await render();
    pick(fixture, new File([new Uint8Array(1000)], 'campus', { type }));

    const upload = http.expectOne((r) => r.url.startsWith('/api/SchoolPhotos') && r.method === 'POST');
    expect(upload.request.body).toBeTruthy();
    upload.flush(blob('00000000-0000-0000-0000-000000000001'));
    await reloadGallery(http);
  });

  it('refuses a GIF, a PDF, an empty file and a file with no type, and says why', async () => {
    const { fixture, http, element } = await render();

    for (const [type, size] of [
      ['image/gif', 1000],
      ['application/pdf', 1000],
      ['', 1000],
      ['image/png', 0],
    ] as const) {
      pick(fixture, new File([new Uint8Array(size)], 'x', { type }));
      fixture.detectChanges();
      http.expectNone((r) => r.method === 'POST');
    }
    expect(element.textContent).toContain('PNG, JPEG or WebP');
  });

  it('accepts a photo of exactly 2 MB and refuses one byte more', async () => {
    const { fixture, http } = await render();

    pick(fixture, new File([new Uint8Array(PHOTO_MAX_BYTES + 1)], 'big', { type: 'image/png' }));
    http.expectNone((r) => r.method === 'POST');

    pick(fixture, new File([new Uint8Array(PHOTO_MAX_BYTES)], 'exact', { type: 'image/png' }));
    http.expectOne((r) => r.url.startsWith('/api/SchoolPhotos') && r.method === 'POST').flush(blob('00000000-0000-0000-0000-000000000002'));
    await reloadGallery(http);
  });
  it('offers each photo kind with its own label', async () => {
    const { fixture } = await render();
    const labels = PHOTO_KINDS.map((kind) => photoKindLabel(kind));

    expect((fixture.componentInstance as unknown as { kinds: readonly string[] }).kinds).toEqual(PHOTO_KINDS);
    expect(labels.every((text) => text.length > 0)).toBe(true);
    expect(new Set(labels).size).toBe(PHOTO_KINDS.length);
  });
});
