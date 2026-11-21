import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { ToastService } from '../../shared/services/toast.service';
import { API_BASE_URL, PhotoKind, SchoolPhotoDto, SchoolPhotosClient } from '../../web-api-client';
import { PHOTO_KINDS, photoKindLabel } from './school-page.labels';
import { checkPhoto, PHOTO_CAPTION_MAX_LENGTH, PHOTO_TYPES } from './photos.logic';

/** The school's gallery: PNG, JPEG or WebP photos of at most 2 MB, each with a kind and a caption. */
@Component({
  selector: 'app-photos-panel',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSelectModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './photos-panel.component.html',
  styleUrl: './school-page.css',
})
export class PhotosPanelComponent implements OnInit {
  private readonly client = inject(SchoolPhotosClient);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);
  private readonly baseUrl = inject(API_BASE_URL, { optional: true }) ?? '';

  protected readonly kinds = PHOTO_KINDS;
  protected readonly kindLabel = photoKindLabel;
  protected readonly captionMax = PHOTO_CAPTION_MAX_LENGTH;
  protected readonly accept = PHOTO_TYPES.join(',');

  protected readonly form = inject(FormBuilder).nonNullable.group({
    kind: [PhotoKind.Campus, Validators.required],
    caption: ['', Validators.maxLength(PHOTO_CAPTION_MAX_LENGTH)],
  });

  protected readonly photos = signal<readonly SchoolPhotoDto[] | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.client.getSchoolPhotos(undefined).subscribe({
      next: (photos) => this.photos.set(photos),
      error: (error: unknown) => {
        this.photos.set([]);
        this.errors.set(problemMessages(error));
      },
    });
  }

  /** The address of a photo's image. The version changes when the image does, so the browser never shows an old one. */
  protected imageUrl(photo: SchoolPhotoDto): string {
    return `${this.baseUrl}/api/SchoolPhotos/${photo.id}/content?v=${encodeURIComponent(photo.version)}`;
  }

  protected upload(input: HTMLInputElement): void {
    const file = input.files?.item(0) ?? null;
    if (file === null) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      input.value = '';
      return;
    }

    const problem = checkPhoto(file);
    if (problem !== null) {
      this.errors.set([problem]);
      input.value = '';
      return;
    }

    const { kind, caption } = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.client.uploadSchoolPhoto(undefined, undefined, kind, caption.trim() || undefined, undefined, { data: file, fileName: file.name }).subscribe({
      next: () => {
        this.busy.set(false);
        input.value = '';
        this.form.controls.caption.reset('');
        this.toast.success($localize`:@@schoolPage.photoUploaded:Photo added.`);
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        input.value = '';
        this.errors.set(problemMessages(error));
      },
    });
  }

  protected confirmDelete(photo: SchoolPhotoDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@schoolPage.deletePhotoTitle:Delete this photo?`,
      message: $localize`:@@schoolPage.deletePhotoMessage:The photo is removed from the gallery.`,
      confirmLabel: $localize`:@@schoolPage.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.busy.set(true);
          this.client.deleteSchoolPhoto(undefined, photo.id).subscribe({
            next: () => {
              this.busy.set(false);
              this.toast.success($localize`:@@schoolPage.photoDeleted:Photo deleted.`);
              this.load();
            },
            error: (error: unknown) => {
              this.busy.set(false);
              this.errors.set(problemMessages(error));
            },
          });
        }
      });
  }
}
