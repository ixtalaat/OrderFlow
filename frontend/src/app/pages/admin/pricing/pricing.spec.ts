import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { MessageService } from 'primeng/api';

import { AdminPricing } from './pricing';

describe('AdminPricing', () => {
  let component: AdminPricing;
  let fixture: ComponentFixture<AdminPricing>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), MessageService],
      imports: [AdminPricing]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminPricing);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
