import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { ConfirmEmail } from './confirm-email';

describe('ConfirmEmail', () => {
  let component: ConfirmEmail;
  let fixture: ComponentFixture<ConfirmEmail>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
      imports: [ConfirmEmail]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ConfirmEmail);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
