import { Component, EventEmitter, Input, Output } from '@angular/core';
import {
  BlockParameterDto,
  EnumBlockParameterDto,
  SettingInputMode,
} from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';

@Component({
  selector: 'app-enum-setting',
  templateUrl: './enum-setting.component.html',
  styleUrls: ['./enum-setting.component.scss'],
})
export class EnumSettingComponent {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;

  changeMode(mode: SettingInputMode) {
    this.setting.inputMode = mode;
    this.onChange.emit();
  }

  valueChanged(value: string) {
    this.setting.value = value;
    this.onChange.emit();
  }

  getOptions() {
    const enumParameter = this.parameter as EnumBlockParameterDto;
    return enumParameter.options;
  }

  // biome-ignore lint/suspicious/noExplicitAny: any
  displayFunction(item: any) {
    return item.toString();
  }
}
