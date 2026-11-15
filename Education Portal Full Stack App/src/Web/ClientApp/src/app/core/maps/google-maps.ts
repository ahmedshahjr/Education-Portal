import { InjectionToken } from '@angular/core';

/** A point on the map, in degrees. */
export interface LatLng {
  lat: number;
  lng: number;
}

/** The part of the Google Maps JavaScript API the campus map picker uses. */
export interface GoogleMapsApi {
  Map: new (element: HTMLElement, options: { center: LatLng; zoom: number; mapTypeControl?: boolean; streetViewControl?: boolean; fullscreenControl?: boolean }) => GoogleMapInstance;
  Marker: new (options: { position: LatLng; map: GoogleMapInstance; draggable: boolean; visible?: boolean }) => GoogleMarkerInstance;
  event: {
    addListener(target: unknown, eventName: string, handler: (event: { latLng?: { lat(): number; lng(): number } }) => void): unknown;
  };
}

export interface GoogleMapInstance {
  panTo(position: LatLng): void;
  setZoom(zoom: number): void;
}

export interface GoogleMarkerInstance {
  setPosition(position: LatLng): void;
  setVisible(visible: boolean): void;
}

/** Keeps one script tag for the page, and lets a failed load be tried again. */
let loading: Promise<GoogleMapsApi> | null = null;

/**
 * Loads the Google Maps JavaScript API with the school's key. The key comes from the server's secret store
 * (GET /api/Campuses/map-settings), so it is never in the repository. Rejects when the script cannot load.
 */
export function loadGoogleMaps(apiKey: string): Promise<GoogleMapsApi> {
  const existing = (window as unknown as { google?: { maps?: GoogleMapsApi } }).google?.maps;
  if (existing) {
    return Promise.resolve(existing);
  }

  loading ??= new Promise<GoogleMapsApi>((resolve, reject) => {
    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${encodeURIComponent(apiKey)}`;
    script.async = true;
    script.onload = () => {
      const maps = (window as unknown as { google?: { maps?: GoogleMapsApi } }).google?.maps;
      if (maps) {
        resolve(maps);
      } else {
        loading = null;
        reject(new Error('Google Maps did not start.'));
      }
    };
    script.onerror = () => {
      script.remove();
      loading = null;
      reject(new Error('Google Maps could not be loaded.'));
    };
    document.head.appendChild(script);
  });

  return loading;
}

/** How the app loads the Maps API. Tests give a fake one through TestBed. */
export const GOOGLE_MAPS_LOADER = new InjectionToken<(apiKey: string) => Promise<GoogleMapsApi>>('GOOGLE_MAPS_LOADER', {
  providedIn: 'root',
  factory: () => loadGoogleMaps,
});
