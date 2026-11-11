import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

/** What the image viewer shows: a translated title and the image's URL. */
export interface ImageViewerData {
  readonly title: string;
  readonly src: string;
}

/** Shows a school's logo or favicon at full size, in a Material dialog (the image is served as an attachment, so a link would download it). */
@Component({
  selector: 'app-image-viewer',
  imports: [MatDialogModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content class="mm-image-viewer">
      <img class="mm-image-viewer__image" [src]="data.src" [alt]="data.title" />
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close i18n="@@branding.closeImage">Close</button>
    </mat-dialog-actions>
  `,
  styles: `
    .mm-image-viewer {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 120px;
    }

    .mm-image-viewer__image {
      max-width: min(80vw, 640px);
      max-height: 70vh;
      object-fit: contain;
    }
  `,
})
export class ImageViewerComponent {
  protected readonly data = inject<ImageViewerData>(MAT_DIALOG_DATA);
}
