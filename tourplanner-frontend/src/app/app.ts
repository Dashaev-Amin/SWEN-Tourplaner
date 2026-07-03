import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TourListComponent } from './components/tour-list/tour-list.component';
import { TourDetailComponent } from './components/tour-detail/tour-detail.component';
import { TourFormComponent } from './components/tour-form/tour-form.component';
import { TourLogListComponent } from './components/tour-log-list/tour-log-list.component';
import { TourLogFormComponent } from './components/tour-log-form/tour-log-form.component';
import { ConfirmDialogComponent } from './components/shared/confirm-dialog/confirm-dialog.component';
import { TourService } from './services/tour.service';
import { TourLogService } from './services/tour-log.service';
import { Tour } from './models/tour.model';
import { TourLog } from './models/tour-log.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    TourListComponent,
    TourDetailComponent,
    TourFormComponent,
    TourLogListComponent,
    TourLogFormComponent,
    ConfirmDialogComponent,
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  tours: Tour[] = [];
  selectedTour: Tour | null = null;
  tourLogs: TourLog[] = [];

  // UI State
  showTourForm = false;
  editingTour: Tour | null = null;
  showLogForm = false;
  editingLog: TourLog | null = null;
  isSaving = false;

  // Confirm Dialog State
  showConfirmDialog = false;
  confirmMessage = '';
  confirmTitle = '';
  confirmAction: (() => void) | null = null;

  constructor(
    private tourService: TourService,
    private tourLogService: TourLogService
  ) {}

  ngOnInit() {
    this.loadTours();
  }

  loadTours() {
    this.tourService.getTours().subscribe({
      next: (tours) => {
        this.tours = tours;
      },
      error: (err) => {
        console.error('Fehler beim Laden der Touren:', err);
        alert('Fehler beim Laden der Touren');
      }
    });
  }

  onSearch(query: string) {
    if (!query) {
      this.loadTours();
      return;
    }
    this.tourService.searchTours(query).subscribe({
      next: (tours) => {
        this.tours = tours;
      },
      error: (err) => {
        console.error('Fehler bei der Suche:', err);
      }
    });
  }

  onTourSelected(tour: Tour) {
    this.selectedTour = tour;
    this.showTourForm = false;
    this.showLogForm = false;
    this.loadLogs(tour.id);
  }

  loadLogs(tourId: string) {
    this.tourLogService.getLogsByTour(tourId).subscribe({
      next: (logs) => {
        this.tourLogs = logs;
      },
      error: (err) => {
        console.error('Fehler beim Laden der Logs:', err);
      }
    });
  }

  private refreshSelectedTour() {
    if (!this.selectedTour) return;
    this.tourService.getTour(this.selectedTour.id).subscribe({
      next: (tour) => {
        this.selectedTour = tour;
        this.loadTours();
      }
    });
  }

  // Export / Import / GPX
  onExport() {
    this.tourService.exportTours();
  }

  onImport(file: File) {
    this.tourService.importTours(file).subscribe({
      next: (imported) => {
        this.loadTours();
        alert(`${imported.length} Tour(en) erfolgreich importiert.`);
      },
      error: (err) => {
        console.error('Fehler beim Import:', err);
        const msg = err.error?.error || 'Unbekannter Fehler beim Import';
        alert('Import fehlgeschlagen: ' + msg);
      }
    });
  }

  onDownloadGpx(tour: Tour) {
    this.tourService.downloadGpx(tour.id);
  }

  // Tour CRUD
  onCreateTour() {
    this.editingTour = null;
    this.showTourForm = true;
  }

  onEditTour(tour: Tour) {
    this.editingTour = tour;
    this.showTourForm = true;
  }

  onDeleteTour(tour: Tour) {
    this.confirmTitle = 'Tour loeschen';
    this.confirmMessage = `Willst du die Tour "${tour.name}" wirklich loeschen?`;
    this.confirmAction = () => {
      if (this.isSaving) return;
      this.isSaving = true;
      this.tourService.deleteTour(tour.id).subscribe({
        next: () => {
          this.selectedTour = null;
          this.tourLogs = [];
          this.loadTours();
          this.isSaving = false;
        },
        error: (err) => {
          console.error('Fehler beim Loeschen:', err);
          alert('Fehler beim Loeschen der Tour');
          this.isSaving = false;
        }
      });
    };
    this.showConfirmDialog = true;
  }

  onTourFormSave(data: Partial<Tour>) {
    if (this.isSaving) return;
    this.isSaving = true;

    if (this.editingTour) {
      this.tourService.updateTour(this.editingTour.id, data).subscribe({
        next: (updated) => {
          this.showTourForm = false;
          this.selectedTour = updated;
          this.loadTours();
          this.isSaving = false;
        },
        error: (err) => {
          console.error('Fehler beim Update:', err);
          alert('Fehler beim Speichern');
          this.isSaving = false;
        }
      });
    } else {
      this.tourService.createTour(data).subscribe({
        next: (created) => {
          this.showTourForm = false;
          this.selectedTour = created;
          this.loadTours();
          this.loadLogs(created.id);
          this.isSaving = false;
        },
        error: (err) => {
          console.error('Fehler beim Erstellen:', err);
          alert('Fehler beim Erstellen');
          this.isSaving = false;
        }
      });
    }
  }

  onTourFormCancel() {
    this.showTourForm = false;
  }

  // Log CRUD
  onCreateLog() {
    this.editingLog = null;
    this.showLogForm = true;
  }

  onEditLog(log: TourLog) {
    this.editingLog = log;
    this.showLogForm = true;
  }

  onDeleteLog(log: TourLog) {
    this.confirmTitle = 'Log loeschen';
    this.confirmMessage = 'Willst du diesen Log wirklich loeschen?';
    this.confirmAction = () => {
      if (!this.selectedTour || this.isSaving) return;
      this.isSaving = true;
      this.tourLogService.deleteLog(this.selectedTour.id, log.id).subscribe({
        next: () => {
          this.loadLogs(this.selectedTour!.id);
          this.refreshSelectedTour();
          this.isSaving = false;
        },
        error: (err) => {
          console.error('Fehler beim Loeschen des Logs:', err);
          alert('Fehler beim Loeschen');
          this.isSaving = false;
        }
      });
    };
    this.showConfirmDialog = true;
  }

  onLogFormSave(data: Partial<TourLog>) {
    if (!this.selectedTour || this.isSaving) return;
    this.isSaving = true;
    const tourId = this.selectedTour.id;

    if (this.editingLog) {
      this.tourLogService.updateLog(tourId, this.editingLog.id, data).subscribe({
        next: () => {
          this.showLogForm = false;
          this.loadLogs(tourId);
          this.refreshSelectedTour();
          this.isSaving = false;
        },
        error: (err) => {
          console.error('Fehler beim Update:', err);
          alert('Fehler beim Speichern');
          this.isSaving = false;
        }
      });
    } else {
      this.tourLogService.createLog(tourId, data).subscribe({
        next: () => {
          this.showLogForm = false;
          this.loadLogs(tourId);
          this.refreshSelectedTour();
          this.isSaving = false;
        },
        error: (err) => {
          console.error('Fehler beim Erstellen:', err);
          alert('Fehler beim Erstellen');
          this.isSaving = false;
        }
      });
    }
  }

  onLogFormCancel() {
    this.showLogForm = false;
  }

  // Confirm Dialog
  onConfirmResult(result: boolean) {
    this.showConfirmDialog = false;
    const action = this.confirmAction;
    this.confirmAction = null;
    if (result && action) {
      action();
    }
  }
}
