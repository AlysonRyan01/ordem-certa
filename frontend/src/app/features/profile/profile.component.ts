import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { NgxMaskDirective, provideNgxMask } from 'ngx-mask';
import { CompanyOutput, ConfirmPasswordChangeInput, UpdateCompanyInput } from '../../core/models/company.model';
import { CompanyService } from '../../core/services/company.service';
import { SkeletonComponent } from '../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    NgxMaskDirective,
    SkeletonComponent,
  ],
  providers: [provideNgxMask()],
  templateUrl: './profile.component.html',
})
export class ProfileComponent implements OnInit {
  private readonly companyService = inject(CompanyService);
  private readonly snackBar = inject(MatSnackBar);

  readonly company = signal<CompanyOutput | null>(null);
  readonly loading = signal(true);
  readonly saving = signal(false);

  readonly passwordStep = signal<'idle' | 'code-sent' | 'saving'>('idle');
  readonly requestingCode = signal(false);

  readonly companyForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    phone: new FormControl('', [Validators.required]),
    street: new FormControl(''),
    number: new FormControl(''),
    city: new FormControl(''),
    state: new FormControl('', [Validators.maxLength(2)]),
  });

  readonly passwordForm = new FormGroup({
    code: new FormControl('', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]),
    newPassword: new FormControl('', [Validators.required, Validators.minLength(6)]),
  });

  ngOnInit(): void {
    this.companyService.getMe().subscribe({
      next: (c) => {
        this.company.set(c);
        this.companyForm.patchValue({
          name: c.name,
          phone: c.phone,
          street: c.street ?? '',
          number: c.number ?? '',
          city: c.city ?? '',
          state: c.state ?? '',
        });
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  saveCompany(): void {
    if (this.companyForm.invalid || this.saving()) return;
    this.saving.set(true);

    const v = this.companyForm.value;
    const input: UpdateCompanyInput = {
      name: v.name!,
      phone: v.phone!,
      street: v.street || undefined,
      number: v.number || undefined,
      city: v.city || undefined,
      state: v.state || undefined,
    };

    this.companyService.updateMe(input).subscribe({
      next: (c) => {
        this.company.set(c);
        this.snackBar.open('Dados da empresa atualizados.', 'Fechar', { duration: 3000 });
        this.saving.set(false);
      },
      error: () => this.saving.set(false),
    });
  }

  requestCode(): void {
    if (this.requestingCode()) return;
    this.requestingCode.set(true);

    this.companyService.requestPasswordChange().subscribe({
      next: () => {
        this.passwordStep.set('code-sent');
        this.requestingCode.set(false);
        this.snackBar.open('Código enviado por WhatsApp!', 'Fechar', { duration: 4000 });
      },
      error: () => this.requestingCode.set(false),
    });
  }

  confirmPasswordChange(): void {
    if (this.passwordForm.invalid || this.passwordStep() === 'saving') return;
    this.passwordStep.set('saving');

    const input: ConfirmPasswordChangeInput = {
      code: this.passwordForm.value.code!,
      newPassword: this.passwordForm.value.newPassword!,
    };

    this.companyService.confirmPasswordChange(input).subscribe({
      next: () => {
        this.passwordStep.set('idle');
        this.passwordForm.reset();
        this.snackBar.open('Senha alterada com sucesso!', 'Fechar', { duration: 3000 });
      },
      error: () => this.passwordStep.set('code-sent'),
    });
  }
}
