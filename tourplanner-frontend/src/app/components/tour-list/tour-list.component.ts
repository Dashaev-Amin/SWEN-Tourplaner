import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tour } from '../../models/tour.model';

@Component({
  selector: 'app-tour-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tour-list.component.html',
  styleUrls: ['./tour-list.component.css']
})
export class TourListComponent {
  @Input() tours: Tour[] = [];
  @Input() selectedTourId: string | null = null;
  @Output() tourSelected = new EventEmitter<Tour>();
  @Output() createNew = new EventEmitter<void>();

  selectTour(tour: Tour) {
    console.log('Tour selected:', tour.name);
    this.tourSelected.emit(tour);
  }

  onCreateNew() {
    this.createNew.emit();
  }
}
