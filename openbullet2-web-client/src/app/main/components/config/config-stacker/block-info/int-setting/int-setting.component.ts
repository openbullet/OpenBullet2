import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';

@Component({
  selector: 'app-int-setting',
  templateUrl: './int-setting.component.html',
  styleUrls: ['./int-setting.component.scss'],
})
export class IntSettingComponent {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;

  changeMode(mode: SettingInputMode) {
    this.setting.inputMode = mode;
    this.onChange.emit();
  }

  setValue(event: any) {
    if (isNaN(event) || isNaN(parseInt(event))) {
      return;
    }

    const value = Number.parseInt(event);

    if (value < -2147483648 || value > 2147483647) {
      return;
    }

    this.setting.value = value;
    this.valueChanged();
  }

  valueChanged() {
    this.onChange.emit();
  }
}
