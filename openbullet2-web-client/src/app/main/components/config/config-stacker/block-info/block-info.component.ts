import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BlockDescriptorDto } from 'src/app/main/dtos/config/block-descriptor.dto';
import { BlockInstanceTypes } from 'src/app/main/dtos/config/block-instance.dto';

@Component({
  selector: 'app-block-info',
  templateUrl: './block-info.component.html',
  styleUrls: ['./block-info.component.scss']
})
export class BlockInfoComponent {
  @Input() block!: BlockInstanceTypes;
  @Input() descriptor!: BlockDescriptorDto;
  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  valueChanged() {
    this.onChange.emit();
    console.log('changed');
  }
}
