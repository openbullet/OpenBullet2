import { Component, EventEmitter, Input, Output } from '@angular/core';
import { faCode, faFont } from '@fortawesome/free-solid-svg-icons';
import { SettingInputMode } from 'src/app/main/dtos/config/block-descriptor.dto';

@Component({
  selector: 'app-setting-input-mode',
  templateUrl: './setting-input-mode.component.html',
  styleUrls: ['./setting-input-mode.component.scss'],
})
export class SettingInputModeComponent {
  @Input() mode!: SettingInputMode;
  @Input() allowedModes!: SettingInputMode[];
  @Output() onChange: EventEmitter<SettingInputMode> = new EventEmitter<SettingInputMode>();

  SettingInputMode = SettingInputMode;

  faFont = faFont;
  faCode = faCode;

  getNextMode() {
    let index = this.allowedModes.indexOf(this.mode);
    index++;

    if (index >= this.allowedModes.length) {
      index = 0;
    }

    return this.allowedModes[index];
  }

  loopMode() {
    this.mode = this.getNextMode();
    this.onChange.emit(this.mode);
  }
}
