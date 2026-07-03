import {
  Component, Input, Output, EventEmitter,
  OnChanges, SimpleChanges, AfterViewInit, ElementRef, ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tour } from '../../models/tour.model';
import * as L from 'leaflet';

@Component({
  selector: 'app-tour-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tour-detail.component.html',
  styleUrls: ['./tour-detail.component.css']
})
export class TourDetailComponent implements OnChanges, AfterViewInit {
  @Input() tour: Tour | null = null;
  @Output() editTour = new EventEmitter<Tour>();
  @Output() deleteTour = new EventEmitter<Tour>();
  @Output() downloadGpx = new EventEmitter<Tour>();

  @ViewChild('mapContainer') mapContainer!: ElementRef;

  private map: L.Map | null = null;
  private routeLayer: L.GeoJSON | null = null;

  ngAfterViewInit() {
    // Map wird beim ersten Tour-Wechsel initialisiert
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['tour']) {
      setTimeout(() => this.updateMap(), 0);
    }
  }

  onEdit() {
    if (this.tour) {
      this.editTour.emit(this.tour);
    }
  }

  onDelete() {
    if (this.tour) {
      this.deleteTour.emit(this.tour);
    }
  }

  onDownloadGpx() {
    if (this.tour) {
      this.downloadGpx.emit(this.tour);
    }
  }

  private updateMap() {
    if (!this.tour?.routeGeometry || !this.mapContainer) {
      return;
    }

    // Leaflet default marker icon fix (Angular build issue)
    delete (L.Icon.Default.prototype as any)._getIconUrl;
    L.Icon.Default.mergeOptions({
      iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
      iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
      shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
    });

    if (!this.map) {
      this.map = L.map(this.mapContainer.nativeElement).setView([47.5, 14.5], 7);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; OpenStreetMap contributors'
      }).addTo(this.map);
    }

    // Alte Route entfernen
    if (this.routeLayer) {
      this.map.removeLayer(this.routeLayer);
    }

    try {
      const geometry = JSON.parse(this.tour.routeGeometry);
      this.routeLayer = L.geoJSON(geometry, {
        style: { color: '#2196F3', weight: 4, opacity: 0.8 }
      }).addTo(this.map);

      // Auf Route zoomen
      this.map.fitBounds(this.routeLayer.getBounds(), { padding: [30, 30] });
    } catch (e) {
      console.error('Fehler beim Parsen der RouteGeometry:', e);
    }
  }
}
