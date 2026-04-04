import { Component, Input, Output, EventEmitter, OnInit, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Tour } from '../../models/tour.model';

@Component({
  selector: 'app-tour-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './tour-form.component.html',
  styleUrls: ['./tour-form.component.css']
})
export class TourFormComponent implements OnInit, OnChanges {
  @Input() tour: Tour | null = null; // null = create, sonst edit
  @Output() save = new EventEmitter<Partial<Tour>>();
  @Output() cancel = new EventEmitter<void>();

  form!: FormGroup;

  transportTypes = ['bike', 'hike', 'running', 'vacation'];

  constructor(private fb: FormBuilder) {}

  ngOnInit() {
    this.initForm();
  }

  ngOnChanges() {
    this.initForm();
  }

  initForm() {
    this.form = this.fb.group({
      name: [this.tour?.name || '', Validators.required],
      description: [this.tour?.description || ''],
      from: [this.tour?.from || '', Validators.required],
      to: [this.tour?.to || '', Validators.required],
      transportType: [this.tour?.transportType || 'bike', Validators.required],
      distance: [this.tour?.distance || 0, [Validators.required, Validators.min(0)]],
      estimatedTime: [this.tour?.estimatedTime || 0, [Validators.required, Validators.min(0)]],
    });
  }

  onSubmit() {
    if (this.form.valid) {
      console.log('Form submitted:', this.form.value);
      this.save.emit(this.form.value);
    } else {
      // alle Felder als touched markieren damit Fehler angezeigt werden
      this.form.markAllAsTouched();
    }
  }

  onCancel() {
    this.cancel.emit();
  }
}
