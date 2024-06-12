import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { MonacoEditorModule, NgxMonacoEditorConfig } from 'ngx-monaco-editor-v2';
import { DropdownModule } from 'primeng/dropdown';
import { BooleanIconComponent } from './components/boolean-icon/boolean-icon.component';
import { CodeEditorComponent } from './components/code-editor/code-editor.component';
import { InputDictionaryComponent } from './components/input-dictionary/input-dictionary.component';
import { InputDropdownComponent } from './components/input-dropdown/input-dropdown.component';
import { InputListComponent } from './components/input-list/input-list.component';
import { InputNumberComponent } from './components/input-number/input-number.component';
import { InputTextComponent } from './components/input-text/input-text.component';
import { InputTimeSpanComponent } from './components/input-time-span/input-time-span.component';
import { MultipleSelectorComponent } from './components/multiple-selector/multiple-selector.component';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { registerLoliCode } from './languages/lolicode';
import { registerLoliScript } from './languages/loliscript';
import { BytesPipe } from './pipes/bytes.pipe';
import { MomentPipe } from './pipes/moment.pipe';
import { PascalCasePipe } from './pipes/pascalcase.pipe';
import { TimeSpanPipe } from './pipes/timespan.pipe';
import { TruncatePipe } from './pipes/truncate.pipe';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';
import { DocsButtonComponent } from './components/docs-button/docs-button.component';
import { TooltipModule } from 'primeng/tooltip';

// biome-ignore lint/suspicious/noExplicitAny: monaco
declare const monaco: any;

const monacoConfig: NgxMonacoEditorConfig = {
  onMonacoLoad() {
    registerLoliCode(monaco);
    registerLoliScript(monaco);
  },
  defaultOptions: {
    automaticLayout: true,
    fontFamily: 'Chivo Mono',
    fontLigatures: false,
  },
};

@NgModule({
  declarations: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe,
    PascalCasePipe,
    TimeSpanPipe,
    MomentPipe,
    SafeHtmlPipe,
    InputTextComponent,
    InputNumberComponent,
    InputDropdownComponent,
    CodeEditorComponent,
    InputListComponent,
    BooleanIconComponent,
    MultipleSelectorComponent,
    InputTimeSpanComponent,
    InputDictionaryComponent,
    DocsButtonComponent,
  ],
  imports: [
    CommonModule,
    DropdownModule,
    MonacoEditorModule.forRoot(monacoConfig),
    FormsModule,
    FontAwesomeModule,
    TooltipModule,
  ],
  exports: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe,
    PascalCasePipe,
    TimeSpanPipe,
    MomentPipe,
    SafeHtmlPipe,
    InputTextComponent,
    InputNumberComponent,
    InputDropdownComponent,
    InputListComponent,
    CodeEditorComponent,
    BooleanIconComponent,
    MultipleSelectorComponent,
    InputTimeSpanComponent,
    InputDictionaryComponent,
    DocsButtonComponent,
  ],
})
export class SharedModule { }
