import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  error = '';
  message = '';
  loading = false;
  recovering = false;
  form;

  constructor(private readonly fb: FormBuilder, private readonly auth: AuthService, private readonly router: Router) {
    this.form = this.fb.group({
      login: ['', Validators.required],
      senha: ['', Validators.required]
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.error = '';
    this.message = '';
    const value = this.form.getRawValue();
    this.auth.login(value.login ?? '', value.senha ?? '').subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (err) => {
        this.error = err.error?.message ?? 'Falha ao autenticar.';
        this.loading = false;
      }
    });
  }

  forgotPassword(): void {
    const loginOuEmail = this.form.controls.login.value?.trim() ?? '';
    this.error = '';
    this.message = '';

    if (!loginOuEmail) {
      this.error = 'Informe o login ou e-mail para recuperar a senha.';
      return;
    }

    this.recovering = true;
    this.auth.forgotPassword(loginOuEmail).subscribe({
      next: (response) => {
        if (response.success) {
          this.message = `${response.message} Ao receber o token, acesse a tela de redefinição de senha.`;
        } else {
          this.error = response.message;
        }
        this.recovering = false;
      },
      error: (err) => {
        this.error = err.error?.message ?? 'Falha ao solicitar recuperação de senha.';
        this.recovering = false;
      }
    });
  }
}
