import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tour } from '../../models/tour.model';

@Component({
  selector: 'app-tour-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tour-detail.component.html',
  styleUrls: ['./tour-detail.component.css']
})
export class TourDetailComponent {
  @Input() tour: Tour | null = null;
  @Output() editTour = new EventEmitter<Tour>();
  @Output() deleteTour = new EventEmitter<Tour>();

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
}
