import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { AuthStore } from '../../core/auth-store';
import { ApiErrorHandler } from '../../core/api-error-handler';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink, Button, Card, InputText, Message, Password],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly auth = inject(AuthStore);
  private readonly errors = inject(ApiErrorHandler);
  private readonly router = inject(Router);

  protected readonly failure = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly form = inject(FormBuilder).group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  protected async submit(): Promise<void> {
    if (this.form.invalid || this.busy()) return;
    this.failure.set(null);
    this.busy.set(true);
    try {
      const { email, password } = this.form.getRawValue();
      await this.auth.login(email!, password!);
      await this.router.navigate(['/']);
    } catch (error: unknown) {
      if (this.errors.toCode(error) === 'Authentication.EmailNotConfirmed') {
        await this.router.navigate(['/confirm-email'], {
          queryParams: { email: this.form.getRawValue().email },
        });
        return;
      }
      this.failure.set(this.errors.toMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}
