import { inject, Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { problemMessages } from '../../core/http/problem-details';

/** How long a toast stays on screen (AC3.1, AC3.2). */
const TOAST_DURATION_MS = 3000;

/** One place for the snackbar's duration and wording (architecture D2). */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly snackBar = inject(MatSnackBar);

  /** Shows a success message. The text must already be translated by the caller. */
  success(message: string): void {
    this.snackBar.open(message, undefined, { duration: TOAST_DURATION_MS });
  }

  /** Shows the first problem message of a failed request. */
  error(problem: unknown): void {
    this.snackBar.open(problemMessages(problem)[0], undefined, { duration: TOAST_DURATION_MS });
  }
}
