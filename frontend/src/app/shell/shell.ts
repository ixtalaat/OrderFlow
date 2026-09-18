import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { Menubar } from 'primeng/menubar';
import { Toast } from 'primeng/toast';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, Menubar, Toast],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell {
  protected readonly menuItems: MenuItem[] = [{ label: 'Home', routerLink: ['/'] }];
}
