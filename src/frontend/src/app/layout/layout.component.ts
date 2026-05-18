import { Component } from '@angular/core';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent {
  menuOpen = false;

  constructor(public readonly auth: AuthService) {}

  get roleLabel(): string {
    return this.auth.role === 'Financeiro' ? 'Custo' : this.auth.role ?? '';
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  closeMenu(): void {
    this.menuOpen = false;
  }
}
