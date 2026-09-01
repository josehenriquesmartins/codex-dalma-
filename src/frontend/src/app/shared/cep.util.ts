import { AbstractControl, ValidationErrors } from '@angular/forms';

export function cepValidator(control: AbstractControl): ValidationErrors | null {
  const value = String(control.value ?? '').trim();
  if (!value) return null;

  return /^\d{5}-?\d{3}$/.test(value) ? null : { cep: true };
}

export function somenteDigitosCep(value: string | null | undefined): string {
  return String(value ?? '').replace(/\D/g, '');
}

export function formatarCep(value: string | null | undefined): string {
  const digits = somenteDigitosCep(value);
  if (digits.length > 8) return String(value ?? '');
  if (digits.length <= 5) return digits;

  return `${digits.slice(0, 5)}-${digits.slice(5)}`;
}
