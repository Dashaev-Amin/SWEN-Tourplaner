import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TourLog } from '../models/tour-log.model';

@Injectable({
  providedIn: 'root'
})
export class TourLogService {
  private baseUrl = '/api/tours';

  constructor(private http: HttpClient) {}

  getLogsByTour(tourId: string): Observable<TourLog[]> {
    return this.http.get<TourLog[]>(`${this.baseUrl}/${tourId}/logs`);
  }

  createLog(tourId: string, log: Partial<TourLog>): Observable<TourLog> {
    return this.http.post<TourLog>(`${this.baseUrl}/${tourId}/logs`, log);
  }

  updateLog(tourId: string, id: string, log: Partial<TourLog>): Observable<TourLog> {
    return this.http.put<TourLog>(`${this.baseUrl}/${tourId}/logs/${id}`, log);
  }

  deleteLog(tourId: string, id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${tourId}/logs/${id}`);
  }
}
