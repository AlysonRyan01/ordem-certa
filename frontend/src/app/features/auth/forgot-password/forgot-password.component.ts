import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './forgot-password.component.html',
})
export class ForgotPasswordComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  readonly step = signal<'email' | 'code'>('email');
  readonly loading = signal(false);
  readonly email = signal('');
  hidePassword = true;

  readonly emailForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  readonly codeForm = this.fb.group({
    code: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
  });

  requestCode(): void {
    if (this.emailForm.invalid || this.loading()) return;
    this.loading.set(true);
    const email = this.emailForm.getRawValue().email!;

    this.auth.requestPasswordReset(email).subscribe({
      next: () => {
        this.email.set(email);
        this.step.set('code');
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  confirmReset(): void {
    if (this.codeForm.invalid || this.loading()) return;
    this.loading.set(true);
    const { code, newPassword } = this.codeForm.getRawValue();

    this.auth.confirmPasswordReset({ email: this.email(), code: code!, newPassword: newPassword! }).subscribe({
      next: () => {
        this.snackBar.open('Senha redefinida com sucesso. Faça login.', 'Fechar', { duration: 5000 });
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }
}
