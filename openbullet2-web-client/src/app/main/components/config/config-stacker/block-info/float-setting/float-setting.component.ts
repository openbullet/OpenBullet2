import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../../config-stacker.component';

@Component({
  selector: 'app-float-setting',
  templateUrl: './float-setting.component.html',
  styleUrls: ['./float-setting.component.scss'],
})
export class FloatSettingComponent {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Input() stacker!: ConfigStackerComponent;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;

  changeMode(mode: SettingInputMode) {
    this.setting.inputMode = mode;
    this.onChange.emit();
  }

  // biome-ignore lint/suspicious/noExplicitAny: This function is only called with events from the input element.
  setValue(event: any) {
    if (Number.isNaN(event) || Number.isNaN(Number.parseFloat(event))) {
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
