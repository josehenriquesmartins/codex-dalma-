import { AbstractControl, ValidationErrors } from '@angular/forms';

/** Mesma regra do backend: local@dominio.tld (TLD com 2+ letras). */
const EMAIL_RE = /^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$/;

export function emailValido(valor: string | null | undefined): boolean {
  return !!valor && EMAIL_RE.test(valor.trim());
}

/** Validador reativo. Vazio é tratado por Validators.required (quando aplicável). Erro: { email: true }. */
export function emailValidator(control: AbstractControl): ValidationErrors | null {
  const valor = control.value;
  if (!valor) { return null; }
  return emailValido(valor) ? null : { email: true };
}
