import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Tour } from '../models/tour.model';

@Injectable({
  providedIn: 'root'
})
export class TourService {
  private baseUrl = '/api/tours';

  constructor(private http: HttpClient) {}

  getTours(): Observable<Tour[]> {
    return this.http.get<Tour[]>(this.baseUrl);
  }

  getTour(id: string): Observable<Tour> {
    return this.http.get<Tour>(`${this.baseUrl}/${id}`);
  }

  createTour(tour: Partial<Tour>): Observable<Tour> {
    return this.http.post<Tour>(this.baseUrl, tour);
  }

  updateTour(id: string, tour: Partial<Tour>): Observable<Tour> {
    return this.http.put<Tour>(`${this.baseUrl}/${id}`, tour);
  }

  deleteTour(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  searchTours(query: string): Observable<Tour[]> {
    return this.http.get<Tour[]>(`${this.baseUrl}/search`, {
      params: { q: query }
    });
  }

  exportTours(): void {
    window.location.href = `${this.baseUrl}/export`;
  }

  importTours(file: File): Observable<Tour[]> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<Tour[]>(`${this.baseUrl}/import`, formData);
  }

  downloadGpx(id: string): void {
    window.location.href = `${this.baseUrl}/${id}/gpx`;
  }
}
