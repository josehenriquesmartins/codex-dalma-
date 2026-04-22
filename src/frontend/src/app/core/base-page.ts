import { FormBuilder } from '@angular/forms';
import { ApiService } from './api.service';

export abstract class BasePageComponent {
  loading = false;
  error = '';

  protected constructor(protected readonly api: ApiService, protected readonly fb: FormBuilder) {}

  protected handleError(error: { error?: { message?: string } }): void {
    this.error = error.error?.message ?? 'Ocorreu um erro ao processar a solicitação.';
    this.loading = false;
  }
}
