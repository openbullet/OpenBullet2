import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../../config-stacker.component';

enum ByteArrayViewMode {
  Base64 = 'base64',
  Hex = 'hex',
}

@Component({
  selector: 'app-byte-array-setting',
  templateUrl: './byte-array-setting.component.html',
  styleUrls: ['./byte-array-setting.component.scss'],
})
export class ByteArraySettingComponent implements OnChanges {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Input() stacker!: ConfigStackerComponent;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;
  ByteArrayViewMode = ByteArrayViewMode;

  viewMode = ByteArrayViewMode.Base64;
  displayValue = '';
  isValid = true;
  isTouched = false;

  private lastBase64Value = '';

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['setting'] && this.getBase64Value() === this.lastBase64Value) {
      return;
    }

    this.syncDisplayValue();
  }

  changeMode(mode: SettingInputMode) {
    this.setting.inputMode = mode;
    this.onChange.emit();
  }

  changeViewMode(mode: ByteArrayViewMode) {
    if (this.viewMode === mode) {
      return;
    }

    this.viewMode = mode;
    this.syncDisplayValue();
  }

  valueChanging(event: Event) {
    this.displayValue = (event.target as HTMLInputElement).value;
    this.applyDisplayValue();
    this.isTouched = true;
  }

  inputChanged(event: Event) {
    this.displayValue = (event.target as HTMLInputElement).value;
    this.applyDisplayValue();
    this.isTouched = true;
  }

  emitValueChanged() {
    this.onChange.emit();
  }

  computeClass(): string {
    let finalClass = 'input-small w-100 monospace';

    if (this.isTouched) {
      finalClass += this.isValid ? ' input-valid' : ' input-invalid';
    }

    return finalClass;
  }

  private applyDisplayValue() {
    const base64Value = this.tryConvertToBase64(this.displayValue);
    this.isValid = base64Value !== null;

    if (base64Value === null) {
      return;
    }

    this.setting.value = base64Value;
    this.lastBase64Value = base64Value;
    this.emitValueChanged();
  }

  private syncDisplayValue() {
    const base64Value = this.getBase64Value();
    this.lastBase64Value = base64Value;
    this.displayValue = this.viewMode === ByteArrayViewMode.Hex
      ? this.base64ToHex(base64Value)
      : base64Value;
    this.isValid = true;
    this.isTouched = false;
  }

  private getBase64Value(): string {
    return typeof this.setting?.value === 'string' ? this.setting.value : '';
  }

  private tryConvertToBase64(value: string): string | null {
    if (this.viewMode === ByteArrayViewMode.Hex) {
      return this.tryHexToBase64(value);
    }

    return this.isValidBase64(value) ? value : null;
  }

  private isValidBase64(value: string): boolean {
    return /^([0-9a-zA-Z+/]{4})*(([0-9a-zA-Z+/]{2}==)|([0-9a-zA-Z+/]{3}=))?$/.test(value);
  }

  private tryHexToBase64(value: string): string | null {
    const normalized = value.replace(/\s/g, '').replace(/0x/g, '');

    if (!/^([0-9a-fA-F]{2})*$/.test(normalized)) {
      return null;
    }

    const bytes = [];
    for (let i = 0; i < normalized.length; i += 2) {
      bytes.push(String.fromCharCode(Number.parseInt(normalized.substring(i, i + 2), 16)));
    }

    return btoa(bytes.join(''));
  }

  private base64ToHex(value: string): string {
    if (!this.isValidBase64(value)) {
      return '';
    }

    return Array.from(atob(value), (char) => char.charCodeAt(0).toString(16).padStart(2, '0')).join('');
  }
}
