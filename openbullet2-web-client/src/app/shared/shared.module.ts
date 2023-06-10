import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { TruncatePipe } from './pipes/truncate.pipe';
import { BytesPipe } from './pipes/bytes.pipe';
import { InputTextComponent } from './components/input-text/input-text.component';
import { InputNumberComponent } from './components/input-number/input-number.component';
import { InputDropdownComponent } from './components/input-dropdown/input-dropdown.component';
import { DropdownModule } from 'primeng/dropdown';
import { LolicodeEditorComponent } from './components/code-editor/code-editor.component';
import { MonacoEditorModule, NgxMonacoEditorConfig } from 'ngx-monaco-editor-v2';
import { FormsModule } from '@angular/forms';
import { registerLoliCode } from './languages/lolicode';
import { InputListComponent } from './components/input-list/input-list.component';
import { PascalCasePipe } from './pipes/pascalcase.pipe';
import { BooleanIconComponent } from './components/boolean-icon/boolean-icon.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

declare const monaco: any;

const monacoConfig: NgxMonacoEditorConfig = {
  onMonacoLoad() {
    registerLoliCode(monaco);
  },
  defaultOptions: {
    automaticLayout: true,
    fontFamily: 'Chivo Mono'
  }
};

@NgModule({
  declarations: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe,
    PascalCasePipe,
    InputTextComponent,
    InputNumberComponent,
    InputDropdownComponent,
    LolicodeEditorComponent,
    InputListComponent,
    BooleanIconComponent
  ],
  imports: [
    CommonModule,
    DropdownModule,
    MonacoEditorModule.forRoot(monacoConfig),
    FormsModule,
    FontAwesomeModule
  ],
  exports: [
    SpinnerComponent,
    TruncatePipe,
    BytesPipe,
    PascalCasePipe,
    InputTextComponent,
    InputNumberComponent,
    InputDropdownComponent,
    InputListComponent,
    LolicodeEditorComponent,
    BooleanIconComponent
  ]
})
export class SharedModule { }
