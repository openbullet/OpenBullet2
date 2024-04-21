import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild } from '@angular/core';
import { BlockDescriptorDto } from 'src/app/main/dtos/config/block-descriptor.dto';
import { LoliCodeBlockInstanceDto } from 'src/app/main/dtos/config/block-instance.dto';
import { CodeEditorComponent } from 'src/app/shared/components/code-editor/code-editor.component';

@Component({
  selector: 'app-lolicode-block',
  templateUrl: './lolicode-block.component.html',
  styleUrls: ['./lolicode-block.component.scss'],
})
export class LolicodeBlockComponent implements OnChanges {
  @Input() block!: LoliCodeBlockInstanceDto;
  @Input() descriptor!: BlockDescriptorDto;

  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  @ViewChild('editor')
  editor: CodeEditorComponent | undefined = undefined;

  ngOnChanges(changes: SimpleChanges): void {
    // We need this when switching between two Lolicode blocks,
    // otherwise the editor will not update the code.
    if (this.editor !== undefined) {
      this.editor.code = this.block.script;
    }
  }

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
