import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, tap, throwError } from 'rxjs';
import { ErrorService } from './error.service';

@Injectable()
export class ApiErrorInterceptor implements HttpInterceptor {
  constructor(private readonly errors: ErrorService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    return next.handle(req).pipe(
      tap((event) => {
        if (event instanceof HttpResponse && this.deveNotificarSucesso(req)) {
          this.errors.sucesso(this.mensagemSucesso(req));
        }
      }),
      catchError((error: HttpErrorResponse) => {
        this.errors.show(this.getMessage(error));
        return throwError(() => error);
      })
    );
  }

  private deveNotificarSucesso(req: HttpRequest<unknown>): boolean {
    const metodo = req.method.toUpperCase();
    if (metodo === 'GET' || metodo === 'HEAD' || metodo === 'OPTIONS') return false;
    // Não notificar transações de autenticação (login, troca/reset de senha).
    if (req.url.includes('/auth')) return false;
    return true;
  }

  private mensagemSucesso(req: HttpRequest<unknown>): string {
    switch (req.method.toUpperCase()) {
      case 'POST':
        return 'Registro salvo com sucesso.';
      case 'PUT':
      case 'PATCH':
        return 'Registro atualizado com sucesso.';
      case 'DELETE':
        return 'Registro excluído com sucesso.';
      default:
        return 'Operação concluída com sucesso.';
    }
  }

  private getMessage(error: HttpErrorResponse): string {
    if (error.error?.message) {
      return error.error.message;
    }

    if (typeof error.error === 'string' && error.error.trim()) {
      return error.error;
    }

    if (error.status === 0) {
      return 'Não foi possível conectar ao servidor. Verifique se a API está em execução.';
    }

    return 'Não foi possível concluir a operação. Revise os dados e tente novamente.';
  }
}
