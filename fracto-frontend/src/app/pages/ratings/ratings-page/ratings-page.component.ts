import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { RatingService } from '../../../core/services/rating.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-ratings-page',
  templateUrl: './ratings-page.component.html'
})
export class RatingsPageComponent implements OnInit {
  doctorId!: number;
  form!: FormGroup;
  average?: number;
  list: any[] = [];
  submitting = false;

  constructor(
    private route: ActivatedRoute,
    private ratingSvc: RatingService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({ score: [5, [Validators.required]] });
    this.doctorId = Number(this.route.snapshot.paramMap.get('doctorId'));
    this.refresh();
  }

  refresh() {
    this.ratingSvc.getAverage(this.doctorId).subscribe(r => this.average = r.average);
    this.ratingSvc.getDoctorRatings(this.doctorId).subscribe((r: any) => this.list = r);
  }

  submit() {
    if (this.form.invalid) return;
    this.submitting = true;
    this.ratingSvc.rate({
      appointmentId: 0, 
      userId: 0,        
      doctorId: this.doctorId,
      score: Number(this.form.value.score)
    }).subscribe({
      next: _ => { this.submitting = false; this.refresh(); },
      error: _ => { this.submitting = false; }
    });
  }
}