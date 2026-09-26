import { Component, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { DashboardService } from '../../../core/dashboard.service';
import { ApiErrorHandler } from '../../../core/api-error-handler';
import { DashboardStats } from '../../../core/models/dashboard-stats';

@Component({
  selector: 'app-admin-dashboard',
  imports: [CurrencyPipe, RouterLink, Button, Card, Message],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class AdminDashboard {
  private readonly dashboard = inject(DashboardService);
  private readonly errors = inject(ApiErrorHandler);

  protected readonly stats = signal<DashboardStats | null>(null);
  protected readonly failure = signal<string | null>(null);
  protected readonly loading = signal(false);

  constructor() {
    void this.load();
  }

  private async load(): Promise<void> {
    this.loading.set(true);
    this.failure.set(null);
    try {
      this.stats.set(await firstValueFrom(this.dashboard.stats()));
    } catch (error: unknown) {
      this.failure.set(this.errors.toMessage(error));
    } finally {
      this.loading.set(false);
    }
  }
}
