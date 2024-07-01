import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges, ViewChild } from '@angular/core';
import { faPlus, faX } from '@fortawesome/free-solid-svg-icons';
import { BlockDescriptorDto, VariableType } from 'src/app/main/dtos/config/block-descriptor.dto';
import { Interpreter, ScriptBlockInstanceDto } from 'src/app/main/dtos/config/block-instance.dto';
import { CodeEditorComponent } from 'src/app/shared/components/code-editor/code-editor.component';

@Component({
  selector: 'app-script-block',
  templateUrl: './script-block.component.html',
  styleUrls: ['./script-block.component.scss'],
})
export class ScriptBlockComponent implements OnChanges {
  @Input() block!: ScriptBlockInstanceDto;
  @Input() descriptor!: BlockDescriptorDto;

  @Output() onChange: EventEmitter<void> = new EventEmitter<void>();

  faPlus = faPlus;
  faX = faX;

  @ViewChild('editor')
  editor: CodeEditorComponent | undefined = undefined;
  interpreters: Interpreter[] = [Interpreter.Jint, Interpreter.NodeJS, Interpreter.IronPython];
  variableTypes: VariableType[] = [
    VariableType.String,
    VariableType.Bool,
    VariableType.Int,
    VariableType.Float,
    VariableType.ByteArray,
    VariableType.ListOfStrings,
    VariableType.DictionaryOfStrings,
  ];

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

  getLanguage(interpreter: Interpreter): string {
    switch (interpreter) {
      case Interpreter.Jint:
      case Interpreter.NodeJS:
        return 'javascript';

      case Interpreter.IronPython:
        return 'python';

      default:
        return 'lolicode';
    }
  }

  addOutputVariable() {
    this.block.outputVariables = [
      ...this.block.outputVariables,
      {
        name: 'myResult',
        type: VariableType.String,
      },
    ];
    this.valueChanged();
  }

  removeOutputVariable(index: number) {
    this.block.outputVariables = this.block.outputVariables.filter((_, i) => i !== index);
    this.valueChanged();
  }

  interpreterChanged(newInterpreter: Interpreter) {
    this.block.interpreter = newInterpreter;
    this.editor!.language = this.getLanguage(newInterpreter);
    this.editor!.resetLanguage();
    this.valueChanged();
  }
}
