import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['../login/login.component.css', './reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {
  error = '';
  message = '';
  loading = false;
  form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly auth: AuthService
  ) {
    this.form = this.fb.group({
      token: ['', Validators.required],
      novaSenha: ['', [Validators.required, Validators.minLength(6)]],
      confirmacaoSenha: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.form.patchValue({ token });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = 'Informe o token e a nova senha.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.message = '';
    const value = this.form.getRawValue();

    this.auth.resetPassword(value.token ?? '', value.novaSenha ?? '', value.confirmacaoSenha ?? '').subscribe({
      next: (response) => {
        if (response.success) {
          this.message = response.message;
          this.form.reset();
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = err.error?.message ?? 'Falha ao redefinir senha.';
        this.loading = false;
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
