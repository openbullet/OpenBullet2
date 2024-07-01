import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BlockDescriptorDto } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockInstanceType, BlockInstanceTypes, BlockSettingType } from 'src/app/main/dtos/config/block-instance.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';
import { ConfigStackerComponent } from '../config-stacker.component';

@Component({
  selector: 'app-block-info',
  templateUrl: './block-info.component.html',
  styleUrls: ['./block-info.component.scss'],
})
export class BlockInfoComponent {
  @Input() block!: BlockInstanceTypes;
  @Input() descriptor!: BlockDescriptorDto;
  @Input() envSettings!: EnvironmentSettingsDto;
  @Input() stacker!: ConfigStackerComponent;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  Object = Object;
  BlockSettingType = BlockSettingType;
  BlockInstanceType = BlockInstanceType;

  valueChanged() {
    this.onChange.emit();
  }
}
