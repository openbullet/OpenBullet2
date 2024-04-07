import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { BlockDescriptorDto } from 'src/app/main/dtos/config/block-descriptor.dto';
import { LoliCodeBlockInstanceDto } from 'src/app/main/dtos/config/block-instance.dto';
import { CodeEditorComponent } from 'src/app/shared/components/code-editor/code-editor.component';

@Component({
  selector: 'app-lolicode-block',
  templateUrl: './lolicode-block.component.html',
  styleUrls: ['./lolicode-block.component.scss'],
})
export class LolicodeBlockComponent {
  @Input() block!: LoliCodeBlockInstanceDto;
  @Input() descriptor!: BlockDescriptorDto;

  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  @ViewChild('editor')
  editor: CodeEditorComponent | undefined = undefined;

  valueChanged() {
    this.onChange.emit();
  }

  editorLoaded() {
    if (this.editor !== undefined) {
      this.editor.code = this.block.script;
    }
  }

  codeChanged(code: string) {
    this.block.script = code;
    this.valueChanged();
  }
}
