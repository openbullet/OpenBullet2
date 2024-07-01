import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FieldValidity } from '../../utils/forms';

@Component({
  selector: 'app-input-dropdown',
  templateUrl: './input-dropdown.component.html',
  styleUrls: ['./input-dropdown.component.scss'],
})
export class InputDropdownComponent<T> {
  @Input() id: string | null = null;
  @Input() key!: string;
  @Input() itemClass: string | null = null;
  @Input() optionClass: string | null = null;
  @Input() options: T[] = [];
  @Input() ngModel: T | null = null;
  @Input() displayFunction: ((item: T) => string) | null = null;

  @Output() touched = new EventEmitter();
  @Output() validityChange = new EventEmitter<FieldValidity>();
  @Output() ngModelChange = new EventEmitter<T>();

  isTouched = false;

  // Notifies the subscribers that this input was touched
  notifyTouched() {
    this.touched.emit();
    this.isTouched = true;
  }

  // Called when the value in the input field changed
  valueChanged() {
    this.notifyTouched();
    this.ngModelChange.emit(this.ngModel!);
  }

  computeItemClass(): string {
    let finalClass = this.itemClass ?? '';

    if (this.isTouched) {
      finalClass += ' input-valid';
    }

    return finalClass;
  }
}
