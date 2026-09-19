import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { AppConfigService } from '../../core/app-config.service';
import { ApiErrorHandler } from '../../core/api-error-handler';
import { AuthStore } from '../../core/auth-store';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, Button, Card, InputText, Message, Password],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);
  private readonly auth = inject(AuthStore);
  private readonly errors = inject(ApiErrorHandler);
  private readonly router = inject(Router);

  protected readonly failure = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly form = inject(FormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    fullName: ['', Validators.required],
  });

  protected async submit(): Promise<void> {
    if (this.form.invalid || this.busy()) return;
    this.failure.set(null);
    this.busy.set(true);
    try {
      const { email, password, fullName } = this.form.getRawValue();
      await firstValueFrom(
        this.http.post(`${this.config.apiUrl}/api/auth/register`, {
          email,
          password,
          fullName,
        }),
      );
      await this.router.navigate(['/confirm-email'], { queryParams: { email } });
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}
