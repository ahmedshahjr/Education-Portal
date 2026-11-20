import { TestBed } from '@angular/core/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { describe, expect, it, vi } from 'vitest';
import { ToastService } from './toast.service';

// AC3.1, AC3.2: every outcome is a snackbar for 3 seconds.
describe('ToastService (AC3.1, AC3.2)', () => {
  function setup() {
    const snackBar = { open: vi.fn() };
    TestBed.configureTestingModule({ providers: [{ provide: MatSnackBar, useValue: snackBar }] });
    return { service: TestBed.inject(ToastService), snackBar };
  }

  it('shows a success message for 3000 ms (AC3.1)', () => {
    const { service, snackBar } = setup();
    service.success('Saved.');
    expect(snackBar.open).toHaveBeenCalledWith('Saved.', undefined, { duration: 3000 });
  });

  it('shows the first problem message on error (AC3.2)', () => {
    const { service, snackBar } = setup();
    service.error({ status: 403, response: '{"title":"Forbidden"}' });
    expect(snackBar.open).toHaveBeenCalledWith(expect.any(String), undefined, { duration: 3000 });
    expect(snackBar.open.mock.calls[0][0]).not.toBe('');
  });
});
