import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FieldValidity } from '../../utils/forms';

@Component({
  selector: 'app-input-list',
  templateUrl: './input-list.component.html',
  styleUrls: ['./input-list.component.scss'],
})
export class InputListComponent implements OnChanges {
  @Input() id: string | null = null;
  @Input() key!: string;
  @Input() class: string | null = null;
  // biome-ignore lint/suspicious/noExplicitAny: any
  @Input() style: { [id: string]: any } = {};
  @Input() regex: string | RegExp | null = null;
  @Input() placeholder = '';
  @Input() removeEmptyLines = false;

  // IMPORTANT: I could not call this ngModel since it's using other
  // ngModels inside it and THEY CONFLICT! Otherwise if the inner
  // ngModel changes, its value would be set here too, effectively
  // setting this value to a string instead of a string[]!
  @Input() list: string[] | null = null;
  @Input() disabled = false;

  @Output() touched = new EventEmitter();
  @Output() validityChange = new EventEmitter<FieldValidity>();
  @Output() listChange = new EventEmitter<string[]>();

  isValid = true;
  isTouched = false;
  value = '';

  ngOnChanges(changes: SimpleChanges): void {
    this.value = this.list === null ? '' : this.list.join('\n');
    this.checkValidity(this.value);
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
      if (this.removeEmptyLines) {
        this.listChange.emit(newValue.split('\n').filter((line) => line.trim() !== ''));
      } else {
        this.listChange.emit(newValue.split('\n'));
      }
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
