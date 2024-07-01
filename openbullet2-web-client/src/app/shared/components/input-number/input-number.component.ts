import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FieldValidity } from '../../utils/forms';

@Component({
  selector: 'app-input-number',
  templateUrl: './input-number.component.html',
  styleUrls: ['./input-number.component.scss'],
})
export class InputNumberComponent implements OnInit {
  @Input() id: string | null = null;
  @Input() key!: string;
  @Input() min: number | null = null;
  @Input() max: number | null = null;
  @Input() step = 1.0;
  @Input() integer = true;
  @Input() class: string | null = null;
  // biome-ignore lint/suspicious/noExplicitAny: any
  @Input() style: { [id: string]: any } = {};
  @Input() placeholder: number | string = '';
  @Input() ngModel: number | null = null;

  @Output() touched = new EventEmitter();
  @Output() validityChange = new EventEmitter<FieldValidity>();
  @Output() ngModelChange = new EventEmitter<number>();

  isValid = true;
  isTouched = false;

  ngOnInit(): void {
    this.notifyValidity(this.checkValidity(this.ngModel ?? 0));
  }

  // Notifies the subscribers that this input was touched
  notifyTouched() {
    this.touched.emit();
    this.isTouched = true;
  }

  // Checks the validity of a new value
  checkValidity(value: number): boolean {
    if (this.integer && !Number.isInteger(value)) {
      return false;
    }

    if (this.min !== null && value < this.min) {
      return false;
    }

    if (this.max !== null && value > this.max) {
      return false;
    }

    return true;
  }

  // Notifies the subscribers that the validity of this input changed
  notifyValidity(valid: boolean) {
    this.validityChange.emit({ key: this.key, valid });
    this.isValid = valid;
  }

  // Called when the value in the input field is changing (user typing)
  valueChanging(event: Event) {
    const newValue = Number.parseFloat((event.target as HTMLInputElement).value);

    // Do not notify the validity for each character typed, just
    // check it to set the correct class
    this.isValid = this.checkValidity(newValue);

    this.notifyTouched();
  }

  // Called when the value in the input field changed
  valueChanged(event: Event) {
    const newValue = Number.parseFloat((event.target as HTMLInputElement).value);
    const valid = this.checkValidity(newValue);

    if (valid) {
      this.ngModelChange.emit(newValue);
    }

    this.notifyValidity(valid);
  }

  computeClass(): string {
    let finalClass = this.class ?? '';

    if (this.isTouched) {
      finalClass += this.isValid ? ' input-valid' : ' input-invalid';
    }

    return finalClass;
  }
}
