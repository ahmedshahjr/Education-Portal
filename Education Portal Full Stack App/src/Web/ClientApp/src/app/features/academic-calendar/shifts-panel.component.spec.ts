import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ShiftsPanelComponent } from './shifts-panel.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const campuses = [
  { id: 'campus-1', name: 'North' },
  { id: 'campus-2', name: 'South' },
];
const shifts = [
  { id: 's1', campusId: 'campus-1', name: 'Morning', startTime: '08:00:00', endTime: '13:00:00' },
  { id: 's2', campusId: 'campus-2', name: 'Evening', startTime: '14:00:00', endTime: '18:30:00' },
];

async function render() {
  TestBed.configureTestingModule({
    imports: [ShiftsPanelComponent],
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(ShiftsPanelComponent);
  fixture.componentRef.setInput('campuses', campuses);
  fixture.detectChanges();
  http.expectOne('/api/Shifts').flush(json(shifts));
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.textContent).toContain('Morning');
  });
  return { fixture, http, element };
}

describe('ShiftsPanelComponent', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('lists each shift with its campus and its times without seconds', async () => {
    const { element } = await render();
    expect(element.textContent).toContain('North');
    expect(element.textContent).toContain('18:30');
    expect(element.textContent).not.toContain('18:30:00');
  });

  it('filters the shifts by campus', async () => {
    const { fixture, element } = await render();
    fixture.componentInstance.selectedCampusId.set('campus-2');
    fixture.detectChanges();
    expect(element.textContent).toContain('Evening');
    expect(element.textContent).not.toContain('Morning');
  });

  it('adds a shift to the chosen campus with full times', async () => {
    const { fixture, http } = await render();
    const panel = fixture.componentInstance;

    panel.selectedCampusId.set('campus-2');
    panel.startAdd();
    panel.form.patchValue({ name: 'Afternoon', startTime: '12:00', endTime: '16:00' });
    panel.save();

    const request = http.expectOne({ method: 'POST', url: '/api/Shifts' });
    expect(JSON.parse(request.request.body as string)).toEqual({ campusId: 'campus-2', name: 'Afternoon', startTime: '12:00:00', endTime: '16:00:00' });
    request.flush(json('s3'));
    await vi.waitFor(() => http.expectOne('/api/Shifts').flush(json(shifts)));
  });

  it('does not send a shift that ends before it starts', async () => {
    const { fixture, http } = await render();
    const panel = fixture.componentInstance;

    panel.startAdd();
    panel.form.patchValue({ campusId: 'campus-1', name: 'Late', startTime: '16:00', endTime: '09:00' });
    panel.save();

    expect(panel.form.hasError('timeRange')).toBe(true);
    http.expectNone({ method: 'POST', url: '/api/Shifts' });
  });
});
