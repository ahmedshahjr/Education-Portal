import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { GOOGLE_MAPS_LOADER, GoogleMapsApi } from '../../core/maps/google-maps';
import { API_BASE_URL } from '../../web-api-client';
import { CampusLocationPickerComponent } from './campus-location-picker.component';

/**
 * Renders the picker with the real campus client over HttpClient. The map key comes from the API's answer; the Google
 * Maps script itself is outside the app, so the loader is the only stand-in.
 */
function render(apiKey: string | undefined, loadMaps: (key: string) => Promise<GoogleMapsApi> = () => new Promise(() => {})) {
  TestBed.configureTestingModule({
    imports: [CampusLocationPickerComponent],
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      { provide: API_BASE_URL, useValue: '' },
      { provide: GOOGLE_MAPS_LOADER, useValue: loadMaps },
    ],
  });
  const fixture = TestBed.createComponent(CampusLocationPickerComponent);
  const located = vi.fn();
  fixture.componentInstance.located.subscribe(located);
  fixture.detectChanges();
  TestBed.inject(HttpTestingController)
    .expectOne('/api/Campuses/map-settings')
    .flush(new Blob([JSON.stringify({ apiKey: apiKey ?? null })], { type: 'application/json' }));
  return { fixture, located, element: fixture.nativeElement as HTMLElement };
}

/** Types into one of the two coordinate fields and reports the change, as the browser does. */
function type(element: HTMLElement, label: 'Latitude' | 'Longitude', text: string) {
  const input = [...element.querySelectorAll<HTMLInputElement>('input')].find((i) => i.closest('mat-form-field')?.textContent?.includes(label));
  if (!input) {
    throw new Error(`No ${label} field.`);
  }
  input.value = text;
  input.dispatchEvent(new Event('change'));
}

describe('CampusLocationPickerComponent', () => {
  afterEach(() => TestBed.resetTestingModule());

  it('takes typed coordinates when no map key is set up, rounded to six decimals', async () => {
    const { fixture, located, element } = render(undefined);
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.textContent).toContain('No map is set up');
    });

    type(element, 'Latitude', '24.86079999');
    expect(located).not.toHaveBeenCalled();

    type(element, 'Longitude', '67.00110001');
    expect(located).toHaveBeenCalledOnce();
    expect(located).toHaveBeenCalledWith({ lat: 24.8608, lng: 67.001100 });
  });

  it('refuses a latitude outside -90 to 90 and reports nothing until both fields are valid', async () => {
    const { fixture, located, element } = render(undefined);
    await vi.waitFor(() => fixture.detectChanges());

    type(element, 'Latitude', '91');
    fixture.detectChanges();
    expect(element.textContent).toContain('Latitude must be between -90 and 90.');

    type(element, 'Longitude', '10');
    expect(located).not.toHaveBeenCalled();
  });

  it('reports the place of a click on the map when a key is set up', async () => {
    let clickHandler: ((event: { latLng?: { lat(): number; lng(): number } }) => void) | undefined;
    const fakeMaps = {
      Map: class {
        panTo() {}
        setZoom() {}
      },
      Marker: class {
        setPosition() {}
        setVisible() {}
      },
      event: {
        addListener: (_target: unknown, name: string, handler: typeof clickHandler) => {
          if (name === 'click') {
            clickHandler = handler;
          }
          return {};
        },
      },
    } as unknown as GoogleMapsApi;

    const { fixture, located } = render('AIza-test-key', async () => fakeMaps);
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(clickHandler).toBeDefined();
    });

    clickHandler?.({ latLng: { lat: () => 24.9, lng: () => 67.1 } });

    expect(located).toHaveBeenCalledWith({ lat: 24.9, lng: 67.1 });
  });
});
