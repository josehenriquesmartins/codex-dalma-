import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { catchError, Observable, throwError } from 'rxjs';
import { ErrorService } from './error.service';

@Injectable()
export class ApiErrorInterceptor implements HttpInterceptor {
  constructor(private readonly errors: ErrorService) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    this.errors.clear();

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        this.errors.show(this.getMessage(error));
        return throwError(() => error);
      })
    );
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
