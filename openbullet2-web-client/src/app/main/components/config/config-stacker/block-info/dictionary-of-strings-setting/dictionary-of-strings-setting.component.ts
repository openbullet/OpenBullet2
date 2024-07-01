import { Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../../config-stacker.component';

@Component({
  selector: 'app-dictionary-of-strings-setting',
  templateUrl: './dictionary-of-strings-setting.component.html',
  styleUrls: ['./dictionary-of-strings-setting.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class DictionaryOfStringsSettingComponent {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Input() stacker!: ConfigStackerComponent;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;

  setValue(value: { [key: string]: string }) {
    // If by any change we're getting a string,
    // disregard it
    if (typeof value === 'string') {
      return;
    }

    this.setting.value = value;
    this.valueChanged();
  }

  changeMode(mode: SettingInputMode) {
    this.setting.inputMode = mode;
    this.onChange.emit();
  }

  valueChanged() {
    this.onChange.emit();
  }
}
