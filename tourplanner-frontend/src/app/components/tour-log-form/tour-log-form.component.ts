import { Component, Input, Output, EventEmitter, OnInit, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { TourLog } from '../../models/tour-log.model';

@Component({
  selector: 'app-tour-log-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tour-log-form.component.html',
  styleUrls: ['./tour-log-form.component.css']
})
export class TourLogFormComponent implements OnInit, OnChanges {
  @Input() log: TourLog | null = null; // null = create, sonst edit
  @Output() save = new EventEmitter<Partial<TourLog>>();
  @Output() cancel = new EventEmitter<void>();

  form!: FormGroup;
  difficulties = [1, 2, 3, 4, 5];
  ratings = [1, 2, 3, 4, 5];

  constructor(private fb: FormBuilder) {}

  ngOnInit() {
    this.initForm();
  }

  ngOnChanges() {
    this.initForm();
  }

  initForm() {
    // Datum formatieren fuer date input
    let dateVal = '';
    if (this.log?.dateTime) {
      dateVal = this.log.dateTime.substring(0, 10); // YYYY-MM-DD
    }

    this.form = this.fb.group({
      dateTime: [dateVal || '', Validators.required],
      comment: [this.log?.comment || ''],
      difficulty: [this.log?.difficulty || 1, [Validators.required, Validators.min(1), Validators.max(5)]],
      totalDistance: [this.log?.totalDistance || 0, [Validators.required, Validators.min(0)]],
      totalTime: [this.log?.totalTime || 0, [Validators.required, Validators.min(0)]],
      rating: [this.log?.rating || 1, [Validators.required, Validators.min(1), Validators.max(5)]],
    });
  }

  onSubmit() {
    if (this.form.valid) {
      const val = this.form.value;
      // date input gibt YYYY-MM-DD, wir brauchen ISO string
      val.dateTime = new Date(val.dateTime).toISOString();
      console.log('Log form submitted:', val);
      this.save.emit(val);
    } else {
      this.form.markAllAsTouched();
    }
  }

  onCancel() {
    this.cancel.emit();
  }
}
