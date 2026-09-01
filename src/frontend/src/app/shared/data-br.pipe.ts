import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'dataBr' })
export class DataBrPipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }

    if (value instanceof Date) {
      return this.formatDate(value);
    }

    const isoDate = /^(\d{4})-(\d{2})-(\d{2})/.exec(value.trim());
    if (isoDate) {
      return `${isoDate[3]}/${isoDate[2]}/${isoDate[1]}`;
    }

    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? value : this.formatDate(parsed);
  }

  private formatDate(date: Date): string {
    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(date);
  }
}
