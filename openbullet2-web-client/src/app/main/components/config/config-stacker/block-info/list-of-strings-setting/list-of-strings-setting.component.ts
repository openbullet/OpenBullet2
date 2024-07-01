import { Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../../config-stacker.component';

@Component({
  selector: 'app-list-of-strings-setting',
  templateUrl: './list-of-strings-setting.component.html',
  styleUrls: ['./list-of-strings-setting.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class ListOfStringsSettingComponent {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Input() stacker!: ConfigStackerComponent;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;

  setValue(value: string[]) {
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
