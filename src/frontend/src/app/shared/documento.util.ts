import { AbstractControl, ValidationErrors } from '@angular/forms';

/** Remove pontuação e mantém alfanuméricos em maiúsculo (CPF fica só com dígitos). */
export function normalizarDocumento(valor: string | null | undefined): string {
  return (valor ?? '').replace(/[^0-9A-Za-z]/g, '').toUpperCase();
}

export function isCpfValido(cpf: string | null | undefined): boolean {
  const d = (cpf ?? '').replace(/\D/g, '');
  if (d.length !== 11 || /^(\d)\1{10}$/.test(d)) { return false; }
  const calc = (len: number): number => {
    let soma = 0;
    for (let i = 0; i < len; i++) { soma += parseInt(d[i], 10) * (len + 1 - i); }
    const r = 11 - (soma % 11);
    return r >= 10 ? 0 : r;
  };
  return calc(9) === +d[9] && calc(10) === +d[10];
}

/** Valida CNPJ numérico OU alfanumérico (novo padrão): 12 posições alfanuméricas + 2 dígitos verificadores. */
export function isCnpjValido(cnpj: string | null | undefined): boolean {
  const c = normalizarDocumento(cnpj);
  if (c.length !== 14 || /^(.)\1{13}$/.test(c)) { return false; }
  if (!/^[0-9A-Z]{12}[0-9]{2}$/.test(c)) { return false; }
  const valor = (ch: string): number => ch.charCodeAt(0) - 48; // '0'->0, 'A'->17
  const calc = (len: number): number => {
    const pesos = len === 12
      ? [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]
      : [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    let soma = 0;
    for (let i = 0; i < len; i++) { soma += valor(c[i]) * pesos[i]; }
    const r = soma % 11;
    return r < 2 ? 0 : 11 - r;
  };
  return calc(12) === +c[12] && calc(13) === +c[13];
}

/** Validador reativo: usa o tipoPessoa do formulário pai para exigir CPF ou CNPJ válido. */
export function documentoValidator(control: AbstractControl): ValidationErrors | null {
  const valor = control.value;
  if (!valor) { return null; } // vazio é tratado pelo Validators.required
  const tipo = control.parent?.get('tipoPessoa')?.value;
  const ok = tipo === 'Fisica' ? isCpfValido(valor) : isCnpjValido(valor);
  return ok ? null : { documento: true };
}
