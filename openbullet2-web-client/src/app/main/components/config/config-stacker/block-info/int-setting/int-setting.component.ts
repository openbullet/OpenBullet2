import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../../config-stacker.component';

@Component({
  selector: 'app-int-setting',
  templateUrl: './int-setting.component.html',
  styleUrls: ['./int-setting.component.scss'],
})
export class IntSettingComponent {
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
    if (Number.isNaN(event) || Number.isNaN(Number.parseInt(event))) {
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
