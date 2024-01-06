import { Component, EventEmitter, Input, Output, ViewEncapsulation } from '@angular/core';
import { BlockSettingDto } from 'src/app/main/dtos/config/block-instance.dto';

@Component({
  selector: 'app-setting-input-variable',
  templateUrl: './setting-input-variable.component.html',
  styleUrls: ['./setting-input-variable.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class SettingInputVariableComponent {
  @Input() setting!: BlockSettingDto;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  // TODO: Add auto-suggest for variables

  valueChanged() {
    this.onChange.emit();
  }
}
