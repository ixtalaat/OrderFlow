import { Component, inject } from '@angular/core';
import { LangService } from '../../core/lang.service';
import { Logo } from '../logo/logo';

@Component({
  selector: 'app-auth-aside',
  imports: [Logo],
  templateUrl: './auth-aside.html',
  styleUrl: './auth-aside.scss',
})
export class AuthAside {
  protected readonly lang = inject(LangService);
}
