import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';

interface Feature {
  icon: string;
  title: string;
  text: string;
}

@Component({
  selector: 'app-home',
  imports: [RouterLink, Button, Card],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  protected readonly features: Feature[] = [
    {
      icon: 'pi pi-tags',
      title: 'Live catalog',
      text: 'Search real stock with your own customer pricing on every product.',
    },
    {
      icon: 'pi pi-bolt',
      title: 'Checkout in seconds',
      text: 'Coupons, idempotent orders, and instant confirmation.',
    },
    {
      icon: 'pi pi-truck',
      title: 'Track everything',
      text: 'Follow each order from submitted to completed, or cancel in one tap.',
    },
    {
      icon: 'pi pi-shield',
      title: 'No overselling',
      text: 'Reservations are concurrency-safe, so stock counts stay honest.',
    },
  ];
}
