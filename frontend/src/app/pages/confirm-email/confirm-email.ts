import { Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { AppConfigService } from '../../core/app-config.service';
import { ApiErrorHandler } from '../../core/api-error-handler';

@Component({
  selector: 'app-confirm-email',
  imports: [ReactiveFormsModule, RouterLink, Button, Card, InputText, Message],
  templateUrl: './confirm-email.html',
  styleUrl: './confirm-email.scss',
})
export class ConfirmEmail {
  private readonly http = inject(HttpClient);
  private readonly config = inject(AppConfigService);
  private readonly errors = inject(ApiErrorHandler);
  private readonly router = inject(Router);

  protected readonly failure = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly form = inject(FormBuilder).group({
    email: [inject(ActivatedRoute).snapshot.queryParamMap.get('email') ?? '', Validators.required],
    token: ['', Validators.required],
  });

  protected async submit(): Promise<void> {
    if (this.form.invalid || this.busy()) return;
    this.failure.set(null);
    this.busy.set(true);
    try {
      await firstValueFrom(
        this.http.post(`${this.config.apiUrl}/api/auth/confirm-email`, this.form.getRawValue()),
      );
      await this.router.navigate(['/login']);
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}
