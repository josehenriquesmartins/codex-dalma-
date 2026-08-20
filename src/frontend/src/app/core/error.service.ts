import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ToastTipo = 'sucesso' | 'erro';

export interface Toast {
  id: number;
  texto: string;
  tipo: ToastTipo;
}

@Injectable({ providedIn: 'root' })
export class ErrorService {
  private seq = 0;
  private readonly toastsSubject = new BehaviorSubject<Toast[]>([]);
  readonly toasts$ = this.toastsSubject.asObservable();

  /** Compatibilidade: exibe uma mensagem de erro. */
  show(message: string): void {
    this.push(message, 'erro');
  }

  /** Exibe uma mensagem de sucesso. */
  sucesso(message: string): void {
    this.push(message, 'sucesso');
  }

  /** Mantido por compatibilidade; os toasts se auto-dispensam. */
  clear(): void {
    // no-op intencional: cada toast some sozinho após o tempo definido.
  }

  dismiss(id: number): void {
    this.toastsSubject.next(this.toastsSubject.value.filter((t) => t.id !== id));
  }

  private push(texto: string, tipo: ToastTipo): void {
    if (!texto || !texto.trim()) return;
    const id = ++this.seq;
    this.toastsSubject.next([...this.toastsSubject.value, { id, texto, tipo }]);
    const tempo = tipo === 'erro' ? 7000 : 4000;
    setTimeout(() => this.dismiss(id), tempo);
  }
}
