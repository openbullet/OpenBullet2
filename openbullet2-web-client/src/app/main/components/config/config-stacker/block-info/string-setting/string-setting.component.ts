import { Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';
import { BlockParameterDto, SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';
import { ConfigStackerComponent } from '../../config-stacker.component';

@Component({
  selector: 'app-string-setting',
  templateUrl: './string-setting.component.html',
  styleUrls: ['./string-setting.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class StringSettingComponent {
  @Input() parameter: BlockParameterDto | null = null;
  @Input() setting!: BlockSettingDto;
  @Input() stacker!: ConfigStackerComponent;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  SettingInputMode = SettingInputMode;

  changeMode(mode: SettingInputMode) {
    this.setting.inputMode = mode;
    this.onChange.emit();
  }

  valueChanged() {
    this.onChange.emit();
  }
}
