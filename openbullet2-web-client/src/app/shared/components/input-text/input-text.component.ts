import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FieldValidity } from '../../utils/forms';

@Component({
  selector: 'app-input-text',
  templateUrl: './input-text.component.html',
  styleUrls: ['./input-text.component.scss'],
})
export class InputTextComponent implements OnInit {
  @Input() id: string | null = null;
  @Input() key!: string;
  @Input() class: string | null = null;
  // biome-ignore lint/suspicious/noExplicitAny: any
  @Input() style: { [id: string]: any } = {};
  @Input() regex: string | RegExp | null = null;
  @Input() placeholder = '';
  @Input() ngModel: string | null = null;

  @Output() touched = new EventEmitter();
  @Output() blur = new EventEmitter();
  @Output() validityChange = new EventEmitter<FieldValidity>();
  @Output() ngModelChange = new EventEmitter<string>();

  isValid = true;
  isTouched = false;

  ngOnInit(): void {
    this.notifyValidity(this.checkValidity(this.ngModel ?? ''));
  }

  // Notifies the subscribers that this input was touched
  notifyTouched() {
    this.touched.emit();
    this.isTouched = true;
  }

  // Checks the validity of a new value
  checkValidity(value: string): boolean {
    if (this.regex !== null && !value.match(this.regex)) {
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
    const newValue = (event.target as HTMLInputElement).value;

    // Do not notify the validity for each character typed, just
    // check it to set the correct class
    this.isValid = this.checkValidity(newValue);

    this.notifyTouched();
  }

  // Called when the value in the input field changed
  valueChanged(event: Event) {
    const newValue = (event.target as HTMLInputElement).value;
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
