import { TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, ActivatedRoute, convertToParamMap } from '@angular/router';
import { ManageUsersComponent } from './manage-users.component';

describe('ManageUsersComponent (smoke)', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageUsersComponent], // If NOT standalone: use declarations:[ManageUsersComponent]
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
    const fix = TestBed.createComponent(ManageUsersComponent);
    expect(fix.componentInstance).toBeTruthy();
  });
});