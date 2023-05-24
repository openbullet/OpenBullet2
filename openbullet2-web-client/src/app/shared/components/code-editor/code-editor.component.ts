import { Component } from '@angular/core';

@Component({
  selector: 'app-code-editor',
  templateUrl: './code-editor.component.html',
  styleUrls: ['./code-editor.component.scss']
})
export class LolicodeEditorComponent {
  editorOptions = {theme: 'vs-lolicode', language: 'lolicode'};
  code: string= 'CLOG Yellow "Hi!"';
}
