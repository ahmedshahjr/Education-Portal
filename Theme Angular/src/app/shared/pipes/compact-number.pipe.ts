import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'compactNumber',
  standalone: true,
})
export class CompactNumberPipe implements PipeTransform {
  transform(value: number | null | undefined, prefix = ''): string {
    if (value === null || value === undefined || Number.isNaN(value)) {
      return `${prefix}0`;
    }
    if (Math.abs(value) >= 1_000_000) {
      return `${prefix}${(value / 1_000_000).toFixed(1)}M`;
    }
    if (Math.abs(value) >= 1_000) {
      return `${prefix}${(value / 1_000).toFixed(1)}k`;
    }
    return `${prefix}${value.toLocaleString('en-US', {
      minimumFractionDigits: value > 0 && value < 10 ? 2 : 0,
      maximumFractionDigits: 4,
    })}`;
  }
}
