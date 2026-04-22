import { Component } from '@angular/core';
import { ErrorService } from './core/error.service';

@Component({
  selector: 'app-root',
  template: `
    <div class="app-error-banner" *ngIf="errors.message$ | async as message">
      <span>{{ message }}</span>
      <button type="button" aria-label="Fechar mensagem" (click)="errors.clear()">x</button>
    </div>
    <router-outlet></router-outlet>
  `
})
export class AppComponent {
  constructor(public readonly errors: ErrorService) {}
}
