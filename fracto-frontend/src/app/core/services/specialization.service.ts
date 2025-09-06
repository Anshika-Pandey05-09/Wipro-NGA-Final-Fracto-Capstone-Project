import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

export interface Specialization { specializationId: number; specializationName: string; }

@Injectable({ providedIn: 'root' })
export class SpecializationService {
  private api = `${environment.apiUrl}/specializations`;
  constructor(private http: HttpClient) {}
  getAll() { return this.http.get<Specialization[]>(this.api); }
}
