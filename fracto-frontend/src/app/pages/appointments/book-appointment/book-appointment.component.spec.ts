import { TestBed } from '@angular/core/testing';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter, ActivatedRoute, convertToParamMap } from '@angular/router';
import { BookAppointmentComponent } from './book-appointment.component';

describe('BookAppointmentComponent (smoke)', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BookAppointmentComponent], // If NOT standalone: use declarations:[BookAppointmentComponent]
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        // supply a route param if your component reads one (e.g., doctorId)
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({ doctorId: '1' }) } } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  });

  it('should create', () => {
    const fix = TestBed.createComponent(BookAppointmentComponent);
    expect(fix.componentInstance).toBeTruthy();
  });
});