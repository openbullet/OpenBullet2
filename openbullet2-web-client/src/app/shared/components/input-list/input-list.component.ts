import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FieldValidity } from '../../utils/forms';

@Component({
  selector: 'app-input-list',
  templateUrl: './input-list.component.html',
  styleUrls: ['./input-list.component.scss']
})
export class InputListComponent implements OnChanges {
  @Input() id: string | null = null;
  @Input() key!: string;
  @Input() class: string | null = null;
  @Input() style: { [id: string] : any; } = {};
  @Input() regex: string | RegExp | null = null;
  @Input() placeholder: string = '';
  @Input() ngModel: string[] | null = null;
  @Input() disabled: boolean = false;

  @Output() touched = new EventEmitter();
  @Output() validityChange = new EventEmitter<FieldValidity>();
  @Output() ngModelChange = new EventEmitter<string[]>();

  isValid = true;
  isTouched = false;
  value = '';

  ngOnChanges(changes: SimpleChanges): void {
    this.value = this.ngModel === null ? '' : this.ngModel.join('\n');
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
    this.validityChange.emit({key: this.key, valid});
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
      this.ngModelChange.emit(
        newValue.split('\n').filter(v => v.trim() !== '')
      );
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
