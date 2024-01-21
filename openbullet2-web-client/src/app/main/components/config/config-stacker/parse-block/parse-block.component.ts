import { Component, EventEmitter, Input, Output } from '@angular/core';
import { BlockDescriptorDto, VariableType } from 'src/app/main/dtos/config/block-descriptor.dto';
import { ParseBlockInstanceDto, ParseMode } from 'src/app/main/dtos/config/block-instance.dto';

@Component({
  selector: 'app-parse-block',
  templateUrl: './parse-block.component.html',
  styleUrls: ['./parse-block.component.scss']
})
export class ParseBlockComponent {
  @Input() block!: ParseBlockInstanceDto;
  @Input() descriptor!: BlockDescriptorDto;

  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  ParseMode = ParseMode;
  VariableType = VariableType;

  valueChanged() {
    this.onChange.emit();
  }

  modeChanged(newMode: ParseMode) {
    this.block.mode = newMode;
    this.valueChanged();
  }
}
