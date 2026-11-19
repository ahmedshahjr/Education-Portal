import { ChangeDetectionStrategy, Component, ElementRef, effect, inject, input, linkedSignal, output, signal, viewChild } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { GOOGLE_MAPS_LOADER, GoogleMapInstance, GoogleMapsApi, GoogleMarkerInstance, LatLng } from '../../core/maps/google-maps';
import { CampusesClient } from '../../web-api-client';

/** Where the map opens when a campus has no place yet: the whole world. */
const WORLD_CENTER: LatLng = { lat: 0, lng: 0 };

/** Where the map opens for a campus that has a place: close enough to see the street. */
const CAMPUS_ZOOM = 15;

/** The map state: loading the key, showing the map, or using the typed coordinates only. */
type MapStatus = 'loading' | 'ready' | 'unavailable' | 'failed';

/** Rounds to the precision the form keeps (six decimals, about 0.1 m). */
const roundCoordinate = (value: number): number => Math.round(value * 1_000_000) / 1_000_000;

/** A coordinate typed into a field, or null when it is blank, not a number, or outside its limit. */
function parseCoordinate(text: string, limit: number): number | null {
  const trimmed = text.trim();
  if (trimmed === '') {
    return null;
  }
  const value = Number(trimmed);
  return Number.isFinite(value) && Math.abs(value) <= limit ? roundCoordinate(value) : null;
}

const formatCoordinate = (value: number | null): string => (value === null ? '' : String(value));

/**
 * Places a campus on the map. With a Google Maps key from the server, a click or a drag of the pin sets the place; the
 * latitude and longitude fields always work too, and are the only way when no map is set up or it cannot load. The place is
 * reported with <see cref="located"/>, and the parent form stores it.
 */
@Component({
  selector: 'app-campus-location-picker',
  imports: [MatFormFieldModule, MatInputModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './campus-location-picker.component.html',
  styleUrl: './campus-location-picker.component.css',
})
export class CampusLocationPickerComponent {
  private readonly campusesClient = inject(CampusesClient);
  private readonly loadMaps = inject(GOOGLE_MAPS_LOADER);

  /** The campus's current place, from the form. */
  readonly latitude = input<number | null>(null);
  readonly longitude = input<number | null>(null);

  /** The place the user chose, on the map or in the fields. */
  readonly located = output<LatLng>();

  protected readonly status = signal<MapStatus>('loading');

  /** The text of each field: kept as typed until it is a valid number. */
  protected readonly latitudeText = linkedSignal(() => formatCoordinate(this.latitude()));
  protected readonly longitudeText = linkedSignal(() => formatCoordinate(this.longitude()));

  protected readonly latitudeInvalid = () => this.latitudeText().trim() !== '' && parseCoordinate(this.latitudeText(), 90) === null;
  protected readonly longitudeInvalid = () => this.longitudeText().trim() !== '' && parseCoordinate(this.longitudeText(), 180) === null;

  private readonly canvas = viewChild.required<ElementRef<HTMLElement>>('canvas');
  private readonly map = signal<{ instance: GoogleMapInstance; marker: GoogleMarkerInstance } | null>(null);

  constructor() {
    this.campusesClient.getCampusMapSettings().subscribe({
      next: (settings) => {
        if (settings.apiKey) {
          this.startMap(settings.apiKey);
        } else {
          this.status.set('unavailable');
        }
      },
      error: () => this.status.set('failed'),
    });

    // Moves the pin and the view when the place changes from the form or the fields.
    effect(() => {
      const current = this.map();
      const lat = this.latitude();
      const lng = this.longitude();
      if (!current) {
        return;
      }
      if (lat === null || lng === null) {
        current.marker.setVisible(false);
        return;
      }
      current.marker.setPosition({ lat, lng });
      current.marker.setVisible(true);
      current.instance.panTo({ lat, lng });
    });
  }

  protected onLatitudeChange(text: string): void {
    this.latitudeText.set(text);
    this.reportIfComplete();
  }

  protected onLongitudeChange(text: string): void {
    this.longitudeText.set(text);
    this.reportIfComplete();
  }

  private startMap(apiKey: string): void {
    this.loadMaps(apiKey)
      .then((api) => this.createMap(api))
      .catch(() => this.status.set('failed'));
  }

  private createMap(api: GoogleMapsApi): void {
    const lat = this.latitude();
    const lng = this.longitude();
    const hasPlace = lat !== null && lng !== null;
    const center = hasPlace ? { lat, lng } : WORLD_CENTER;

    const instance = new api.Map(this.canvas().nativeElement, {
      center,
      zoom: hasPlace ? CAMPUS_ZOOM : 2,
      mapTypeControl: false,
      streetViewControl: false,
      fullscreenControl: false,
    });
    const marker = new api.Marker({ position: center, map: instance, draggable: true, visible: hasPlace });

    api.event.addListener(instance, 'click', (event) => {
      if (event.latLng) {
        this.report(event.latLng.lat(), event.latLng.lng());
      }
    });
    api.event.addListener(marker, 'dragend', (event) => {
      if (event.latLng) {
        this.report(event.latLng.lat(), event.latLng.lng());
      }
    });

    this.map.set({ instance, marker });
    this.status.set('ready');
  }

  /** A place picked on the map: the fields show it, and the form takes it. */
  private report(lat: number, lng: number): void {
    const latitude = roundCoordinate(lat);
    const longitude = roundCoordinate(lng);
    this.latitudeText.set(String(latitude));
    this.longitudeText.set(String(longitude));
    this.located.emit({ lat: latitude, lng: longitude });
  }

  /** A place typed in both fields is reported once both are valid. */
  private reportIfComplete(): void {
    const lat = parseCoordinate(this.latitudeText(), 90);
    const lng = parseCoordinate(this.longitudeText(), 180);
    if (lat !== null && lng !== null) {
      this.located.emit({ lat, lng });
    }
  }
}
