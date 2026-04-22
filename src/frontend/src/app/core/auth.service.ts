import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../environments/environment';

export interface AuthResponse {
  token: string;
  nome: string;
  email: string;
  perfil: 'Admin' | 'Financeiro' | 'Fornecedor';
  fornecedorId?: number;
  expiresAtUtc: string;
}

export interface ForgotPasswordResponse {
  message: string;
  success: boolean;
  email?: string;
}

export interface ResetPasswordResponse {
  message: string;
  success: boolean;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly storageKey = 'dalba_auth';

  constructor(private readonly http: HttpClient, private readonly router: Router) {}

  login(login: string, senha: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${environment.apiUrl}/auth/login`, { login, senha }).pipe(
      tap((response) => localStorage.setItem(this.storageKey, JSON.stringify(response)))
    );
  }

  forgotPassword(loginOuEmail: string): Observable<ForgotPasswordResponse> {
    return this.http.post<ForgotPasswordResponse>(`${environment.apiUrl}/auth/forgot-password`, { loginOuEmail });
  }

  resetPassword(token: string, novaSenha: string, confirmacaoSenha: string): Observable<ResetPasswordResponse> {
    return this.http.post<ResetPasswordResponse>(`${environment.apiUrl}/auth/reset-password`, { token, novaSenha, confirmacaoSenha });
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.router.navigate(['/login']);
  }

  get session(): AuthResponse | null {
    const raw = localStorage.getItem(this.storageKey);
    return raw ? JSON.parse(raw) as AuthResponse : null;
  }

  get token(): string | null { return this.session?.token ?? null; }
  get role(): string | null { return this.session?.perfil ?? null; }
  isAuthenticated(): boolean { return !!this.token; }
}
