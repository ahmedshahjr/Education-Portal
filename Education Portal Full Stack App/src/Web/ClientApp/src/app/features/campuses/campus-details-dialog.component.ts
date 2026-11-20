import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { CampusDto, CampusKind } from '../../web-api-client';
import { genderPolicyLabel } from './campus-gender-policy';

/**
 * One campus's details in a dialog: opened from a row of the campuses table. Read only; the table's edit icon changes it.
 */
@Component({
  selector: 'app-campus-details-dialog',
  imports: [MatDialogModule, MatButtonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './campus-details-dialog.component.html',
  styleUrl: './campus-details-dialog.component.css',
})
export class CampusDetailsDialogComponent {
  protected readonly campus = inject<CampusDto>(MAT_DIALOG_DATA);
  protected readonly kinds = CampusKind;
  protected readonly genderLabel = (): string => genderPolicyLabel(this.campus.genderPolicy);

  /** A Google Maps link to the campus's place. Null when the campus has no place. */
  protected mapLink(): string | null {
    const { latitude, longitude } = this.campus.details;
    if (latitude == null || longitude == null) {
      return null;
    }
    return `https://www.google.com/maps?q=${latitude},${longitude}`;
  }
}
