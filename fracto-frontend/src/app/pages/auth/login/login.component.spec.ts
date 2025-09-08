import { TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, ActivatedRoute, convertToParamMap } from '@angular/router';
import { LoginComponent } from './login.component';

describe('LoginComponent (smoke)', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent], // If NOT standalone: use declarations:[LoginComponent]
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({}) } } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  it('should create', () => {
    const fix = TestBed.createComponent(LoginComponent);
    expect(fix.componentInstance).toBeTruthy();
  });
});