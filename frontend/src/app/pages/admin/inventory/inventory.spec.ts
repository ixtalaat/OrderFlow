import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { MessageService } from 'primeng/api';

import { AdminInventory } from './inventory';

describe('AdminInventory', () => {
  let component: AdminInventory;
  let fixture: ComponentFixture<AdminInventory>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([]), MessageService],
      imports: [AdminInventory]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminInventory);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
