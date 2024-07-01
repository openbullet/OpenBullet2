import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../../config-stacker.component';

@Component({
  selector: 'app-byte-array-setting',
  templateUrl: './byte-array-setting.component.html',
  styleUrls: ['./byte-array-setting.component.scss'],
})
export class ByteArraySettingComponent {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Input() stacker!: ConfigStackerComponent;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;

  changeMode(mode: SettingInputMode) {
    this.setting.inputMode = mode;
    this.onChange.emit();
  }

  // Given a base64 string, sets the value of the setting.
  // biome-ignore lint/suspicious/noExplicitAny: This function is only called with events from the input element.
  setValue(event: any) {
    // Make sure the input is a valid base64 string.
    if (
      event === null ||
      event === undefined ||
      !/^([0-9a-zA-Z+/]{4})*(([0-9a-zA-Z+/]{2}==)|([0-9a-zA-Z+/]{3}=))?$/.test(event)
    ) {
      return;
    }

    this.setting.value = event;
    this.valueChanged();
  }

  valueChanged() {
    this.onChange.emit();
  }
}
