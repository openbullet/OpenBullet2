import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { EditorComponent } from 'ngx-monaco-editor-v2';
import { combineLatest } from 'rxjs';
import { ConfigService } from 'src/app/main/services/config.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { autoCompleteBlock, autoCompleteLoliCodeStatement } from '../../languages/lolicode';

// biome-ignore lint/suspicious/noExplicitAny: monaco
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
  // biome-ignore lint/suspicious/noExplicitAny: any
  editorOptions: any = null;
  // biome-ignore lint/suspicious/noExplicitAny: Monaco editor instance
  editorInstance: any = null;
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
    this.settingsService.getSafeSettings().subscribe((settings) => {
      this.editorOptions = {
        theme: this.theme,
        language: this.language,
        readOnly: this.readOnly,
        wordWrap: settings.customizationSettings.wordWrap ? 'on' : 'off',
        fontFamily: 'Chivo Mono',
        fontLigatures: false,
        suggest: {
          showInlineDetails: false,
          showStatusBar: true,
        },
      };
    });
  }

  // biome-ignore lint/suspicious/noExplicitAny: Monaco editor instance
  editorLoaded(editorInstance: any) {
    this.editorInstance = editorInstance;
    monaco.editor.remeasureFonts();
    this.editorInstance.layout();

    if ('fonts' in document) {
      void document.fonts.ready.then(() => {
        monaco.editor.remeasureFonts();
        this.editorInstance?.layout();
      });
    }

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
            // biome-ignore lint/suspicious/noExplicitAny: any
            provideCompletionItems: (model: any, position: any) => {
              // Check if we are completing BLOCK:
              const textUntilPosition = model.getValueInRange({
                startLineNumber: position.lineNumber,
                startColumn: 1,
                endLineNumber: position.lineNumber,
                endColumn: position.column,
              });
              const trimmedText = textUntilPosition.trim();
              const indentationLength = textUntilPosition.match(/^\s*/)?.[0].length ?? 0;

              const word = model.getWordUntilPosition(position);
              const range = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: word.startColumn,
                endColumn: word.endColumn,
              };
              const blockRange = {
                startLineNumber: position.lineNumber,
                endLineNumber: position.lineNumber,
                startColumn: indentationLength + 1,
                endColumn: position.column,
              };
              const isBlockPrefix =
                trimmedText.length >= 'BLOCK'.length && 'BLOCK:'.startsWith(trimmedText.toUpperCase());
              const isBlockContext = trimmedText.toUpperCase().startsWith('BLOCK:');

              if (isBlockPrefix || isBlockContext) {
                const suggestions = [];
                if (!isBlockContext) {
                  suggestions.push({
                    label: 'BLOCK:',
                    kind: monaco.languages.CompletionItemKind.Keyword,
                    insertText: 'BLOCK:',
                    insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
                    range: blockRange,
                    detail: 'Block prefix',
                    documentation: 'BLOCK:',
                  });
                }
                suggestions.push(...autoCompleteBlock(monaco, blockRange));
                return {
                  suggestions,
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
