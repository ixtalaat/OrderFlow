import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';

@Component({
  selector: 'app-not-found',
  imports: [RouterLink, Button, Card],
  templateUrl: './not-found.html',
  styleUrl: './not-found.scss',
})
export class NotFound {}
