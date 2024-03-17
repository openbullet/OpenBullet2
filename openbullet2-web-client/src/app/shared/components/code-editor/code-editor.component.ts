import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { EditorComponent } from 'ngx-monaco-editor-v2';

@Component({
  selector: 'app-code-editor',
  templateUrl: './code-editor.component.html',
  styleUrls: ['./code-editor.component.scss']
})
export class CodeEditorComponent implements OnInit {
  @Input() id: string | null = null;
  @Input() key!: string;
  @Input() language: string = 'lolicode';
  @Input() readOnly: boolean = false;
  @Input() theme: string = 'vs-dark-lolicode';
  editorOptions: any = {};
  isTouched = false;
  model: string = '';

  @Output() touched = new EventEmitter();
  @Output() codeChanged = new EventEmitter<string>();
  @Output() loaded = new EventEmitter();

  @ViewChild('editor')
  editor: EditorComponent | undefined = undefined;

  ngOnInit(): void {
    this.editorOptions = {
      theme: this.theme,
      language: this.language,
      readOnly: this.readOnly,
      // wordWrap: true
    };
  }

  editorLoaded() {
    this.loaded.emit();
  }

  // Notifies the subscribers that this input was touched
  notifyTouched() {
    this.touched.emit();
    this.isTouched = true;
  }

  set code(value: string) {
    this.model = value;

    // Had to add this or it wouldn't work
    this.editor?.writeValue(value);
  }

  get code(): string {
    return this.model;
  }

  valueChanged(newValue: string) {
    this.notifyTouched();
    this.model = newValue;
    this.codeChanged.emit(newValue);
  }

  public resetLanguage() {
    this.editorOptions = { ...this.editorOptions, language: this.language };
  }
}
