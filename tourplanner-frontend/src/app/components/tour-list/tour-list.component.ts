import { Component, Input, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Tour } from '../../models/tour.model';

@Component({
  selector: 'app-tour-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './tour-list.component.html',
  styleUrls: ['./tour-list.component.css']
})
export class TourListComponent {
  @Input() tours: Tour[] = [];
  @Input() selectedTourId: string | null = null;
  @Output() tourSelected = new EventEmitter<Tour>();
  @Output() createNew = new EventEmitter<void>();
  @Output() searchChanged = new EventEmitter<string>();
  @Output() exportTours = new EventEmitter<void>();
  @Output() importFile = new EventEmitter<File>();

  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  searchQuery = '';
  private debounceTimer: any = null;

  onSearchInput() {
    clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(() => {
      this.searchChanged.emit(this.searchQuery.trim());
    }, 300);
  }

  selectTour(tour: Tour) {
    this.tourSelected.emit(tour);
  }

  onCreateNew() {
    this.createNew.emit();
  }

  onExport() {
    this.exportTours.emit();
  }

  onImportClick() {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.importFile.emit(input.files[0]);
      input.value = ''; // Reset damit gleiche Datei nochmal gewaehlt werden kann
    }
  }
}
