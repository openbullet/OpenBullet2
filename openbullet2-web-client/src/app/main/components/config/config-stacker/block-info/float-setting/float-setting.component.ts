import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';

@Component({
  selector: 'app-float-setting',
  templateUrl: './float-setting.component.html',
  styleUrls: ['./float-setting.component.scss'],
})
export class FloatSettingComponent {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;

  changeMode(mode: SettingInputMode) {
    this.setting.inputMode = mode;
    this.onChange.emit();
  }

  setValue(event: any) {
    if (isNaN(event) || isNaN(Number.parseFloat(event))) {
      return;
    }

    const value = Number.parseFloat(event);

    if (value < -3.40282347e38 || value > 3.40282347e38) {
      return;
    }

    this.setting.value = value;
    this.valueChanged();
  }

  valueChanged() {
    this.onChange.emit();
  }
}
