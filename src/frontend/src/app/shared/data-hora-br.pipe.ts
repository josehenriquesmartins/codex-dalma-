import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'dataHoraBr' })
export class DataHoraBrPipe implements PipeTransform {
  transform(value: string | Date | null | undefined): string {
    if (!value) {
      return '';
    }

    // O backend grava os instantes em UTC em colunas sem fuso.
    const normalized = typeof value === 'string' && /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(value) && !/(Z|[+-]\d{2}:?\d{2})$/i.test(value)
      ? `${value}Z`
      : value;
    const parsed = normalized instanceof Date ? normalized : new Date(normalized);
    if (Number.isNaN(parsed.getTime())) {
      return String(value);
    }

    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    }).format(parsed).replace(',', '');
  }
}
