import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TourLog } from '../../models/tour-log.model';

@Component({
  selector: 'app-tour-log-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tour-log-list.component.html',
  styleUrls: ['./tour-log-list.component.css']
})
export class TourLogListComponent {
  @Input() logs: TourLog[] = [];
  @Input() tourId: string | null = null;
  @Output() createLog = new EventEmitter<void>();
  @Output() editLog = new EventEmitter<TourLog>();
  @Output() deleteLog = new EventEmitter<TourLog>();

  onCreateLog() {
    this.createLog.emit();
  }

  onEdit(log: TourLog) {
    this.editLog.emit(log);
  }

  onDelete(log: TourLog) {
    this.deleteLog.emit(log);
  }

  // Kommentar kuerzen fuer die Tabelle
  truncate(text: string, maxLen: number = 30): string {
    if (!text) return '';
    if (text.length <= maxLen) return text;
    return text.substring(0, maxLen) + '...';
  }
}
