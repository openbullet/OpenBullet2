import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { EditorComponent } from 'ngx-monaco-editor-v2';
import { combineLatest } from 'rxjs';
import { ConfigService } from 'src/app/main/services/config.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { autoCompleteBlock, autoCompleteLoliCodeStatement } from '../../languages/lolicode';

declare const monaco: any;

@Component({
  selector: 'app-code-editor',
  templateUrl: './code-editor.component.html',
  styleUrls: ['./code-editor.component.scss'],
})
export class CodeEditorComponent implements OnInit {
  @Input() id: string | null = null;
  @Input() key!: string;
  @Input() language = 'lolicode';
  @Input() readOnly = false;
  @Input() theme = 'vs-dark-lolicode';
  editorOptions: any = {};
  isTouched = false;
  model = '';

  @Output() touched = new EventEmitter();
  @Output() codeChanged = new EventEmitter<string>();
  @Output() loaded = new EventEmitter();

  @ViewChild('editor')
  editor: EditorComponent | undefined = undefined;

  constructor(
    private settingsService: SettingsService,
    private configService: ConfigService,
  ) {}

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

    if (this.language === 'lolicode') {
      if (monaco.loliCodeCompletionsRegistered) {
        return;
      }

      const blockSnippetsObservable = this.configService.getBlockSnippets();
      const customSnippetsObservable = this.settingsService.getCustomSnippets();

      combineLatest([blockSnippetsObservable, customSnippetsObservable]).subscribe(
        ([blockSnippets, customSnippets]) => {
          monaco.loliCodeBlockSnippets = blockSnippets;
          monaco.loliCodeCustomSnippets = customSnippets;

          monaco.languages.registerCompletionItemProvider('lolicode', {
            provideCompletionItems: (model: any, position: any) => {
              // Check if we are completing BLOCK:
              var textUntilPosition = model.getValueInRange({
                startLineNumber: position.lineNumber,
                startColumn: 1,
                endLineNumber: position.lineNumber,
                endColumn: position.column,
              });

              if ('BLOCK:'.startsWith(textUntilPosition.trim())) {
                return {
                  suggestions: [
                    {
                      label: 'BLOCK:',
                      kind: monaco.languages.CompletionItemKind.Snippet,
                      insertText: 'BLOCK:',
                      insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    },
                  ],
                };
              }

              var word = model.getWordUntilPosition(position);
              var range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn,
              };

              if (textUntilPosition.trim().startsWith('BLOCK:')) {
                return {
                  suggestions: autoCompleteBlock(monaco, range),
                };
              }

              return {
                suggestions: autoCompleteLoliCodeStatement(monaco, range),
              };
            },
          });

          monaco.loliCodeCompletionsRegistered = true;
        },
      );
    }
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
