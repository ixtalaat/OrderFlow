import { Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { LangService } from '../../core/lang.service';

interface Feature {
  icon: string;
  title: string;
  text: string;
}

interface Step {
  title: string;
  text: string;
}

interface Testimonial {
  quote: string;
  name: string;
  role: string;
}

@Component({
  selector: 'app-home',
  imports: [RouterLink, Button, Card],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  protected readonly lang = inject(LangService);

  protected readonly features = computed<Feature[]>(() => [
    { icon: 'pi pi-tags', title: this.lang.t('home.f1t'), text: this.lang.t('home.f1d') },
    { icon: 'pi pi-bolt', title: this.lang.t('home.f2t'), text: this.lang.t('home.f2d') },
    { icon: 'pi pi-truck', title: this.lang.t('home.f3t'), text: this.lang.t('home.f3d') },
    { icon: 'pi pi-shield', title: this.lang.t('home.f4t'), text: this.lang.t('home.f4d') },
  ]);

  protected readonly steps = computed<Step[]>(() => [
    { title: this.lang.t('home.s1t'), text: this.lang.t('home.s1d') },
    { title: this.lang.t('home.s2t'), text: this.lang.t('home.s2d') },
    { title: this.lang.t('home.s3t'), text: this.lang.t('home.s3d') },
  ]);

  protected readonly testimonials = computed<Testimonial[]>(() => [
    { quote: this.lang.t('home.t1q'), name: this.lang.t('home.t1n'), role: this.lang.t('home.t1r') },
    { quote: this.lang.t('home.t2q'), name: this.lang.t('home.t2n'), role: this.lang.t('home.t2r') },
    { quote: this.lang.t('home.t3q'), name: this.lang.t('home.t3n'), role: this.lang.t('home.t3r') },
  ]);

  protected readonly popularSearches: string[] = [
    'Widgets',
    'Gadgets',
    'Accessories',
    'New arrivals',
  ];
}
