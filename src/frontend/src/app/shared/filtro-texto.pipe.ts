import { Pipe, PipeTransform } from '@angular/core';

/**
 * Filtro de texto genérico: mantém os itens cujo conteúdo (qualquer campo string/número)
 * contém o termo informado. Ignora acentuação e maiúsculas/minúsculas.
 * Uso: `*ngFor="let x of lista | filtroTexto:termo"`.
 */
@Pipe({ name: 'filtroTexto' })
export class FiltroTextoPipe implements PipeTransform {
  private static readonly DIACRITICOS = /[̀-ͯ]/g;

  transform<T>(itens: T[] | null | undefined, termo: string | null | undefined): T[] {
    if (!itens) { return []; }
    const alvo = this.normalizar(termo ?? '');
    if (!alvo) { return itens; }
    return itens.filter((item) => this.textoDoItem(item).includes(alvo));
  }

  private textoDoItem(item: unknown): string {
    if (item === null || item === undefined) { return ''; }
    const valores = Object.values(item as Record<string, unknown>)
      .filter((v) => typeof v === 'string' || typeof v === 'number')
      .join(' ');
    return this.normalizar(valores);
  }

  private normalizar(valor: string | number): string {
    return String(valor)
      .normalize('NFD')
      .replace(FiltroTextoPipe.DIACRITICOS, '')
      .toLowerCase()
      .trim();
  }
}
