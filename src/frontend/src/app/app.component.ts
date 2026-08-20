import { Component } from '@angular/core';
import { ErrorService } from './core/error.service';

@Component({
  selector: 'app-root',
  template: `
    <div class="toast-stack">
      <div *ngFor="let t of errors.toasts$ | async" class="toast-item" [class.sucesso]="t.tipo === 'sucesso'" [class.erro]="t.tipo === 'erro'">
        <span class="toast-icon">{{ t.tipo === 'sucesso' ? '✓' : '!' }}</span>
        <span class="toast-text">{{ t.texto }}</span>
        <button type="button" aria-label="Fechar mensagem" (click)="errors.dismiss(t.id)">✕</button>
      </div>
    </div>
    <router-outlet></router-outlet>
  `
})
export class AppComponent {
  constructor(public readonly errors: ErrorService) {}
}
