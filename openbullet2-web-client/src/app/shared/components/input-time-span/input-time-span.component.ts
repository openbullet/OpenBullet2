import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FieldValidity } from '../../utils/forms';
import { TimeSpan } from '../../utils/timespan';

@Component({
  selector: 'app-input-time-span',
  templateUrl: './input-time-span.component.html',
  styleUrls: ['./input-time-span.component.scss'],
})
export class InputTimeSpanComponent {
  @Input() id: string | null = null;
  @Input() key!: string;

  // IMPORTANT: I could not call this ngModel since it's using other
  // ngModels inside it and THEY CONFLICT! Otherwise if the inner
  // ngModel changes, its value would be set here too, effectively
  // setting this value to a number instead of a TimeSpan!
  @Input() timeSpan: TimeSpan = new TimeSpan(0);
  @Input() small = true;

  @Output() touched = new EventEmitter();
  @Output() validityChange = new EventEmitter<FieldValidity>();
  @Output() timeSpanChange = new EventEmitter<TimeSpan>();

  isValid = true;
  isTouched = false;

  // Notifies the subscribers that this input was touched
  notifyTouched() {
    this.touched.emit();
    this.isTouched = true;
  }

  validityChanged(validity: FieldValidity) {
    this.isValid = validity.valid;
    this.validityChange.emit({ key: this.key, valid: this.isValid });
  }

  secondsChanging(value: number) {
    this.timeSpan.seconds = value;
    this.timeSpan = new TimeSpan(this.timeSpan.totalMilliseconds);
    this.notifyTouched();
    this.timeSpanChange.emit(this.timeSpan);
  }

  minutesChanging(value: number) {
    this.timeSpan.minutes = value;
    this.timeSpan = new TimeSpan(this.timeSpan.totalMilliseconds);
    this.notifyTouched();
    this.timeSpanChange.emit(this.timeSpan);
  }

  hoursChanging(value: number) {
    this.timeSpan.hours = value;
    this.timeSpan = new TimeSpan(this.timeSpan.totalMilliseconds);
    this.notifyTouched();
    this.timeSpanChange.emit(this.timeSpan);
  }

  daysChanging(value: number) {
    this.timeSpan.days = value;
    this.timeSpan = new TimeSpan(this.timeSpan.totalMilliseconds);
    this.notifyTouched();
    this.timeSpanChange.emit(this.timeSpan);
  }
}
