import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-code-editor',
  templateUrl: './code-editor.component.html',
  styleUrls: ['./code-editor.component.scss']
})
export class LolicodeEditorComponent implements OnInit {
  @Input() id: string | null = null;
  @Input() key!: string;
  @Input() language: string = 'lolicode';
  @Input() ngModel: string | null = null;
  editorOptions: any = {};
  isTouched = false;

  @Output() touched = new EventEmitter();
  @Output() ngModelChange = new EventEmitter<string>();
  
  ngOnInit(): void {
    this.editorOptions = {
      theme: 'vs-lolicode',
      language: this.language
    };
  }

  // Notifies the subscribers that this input was touched
  notifyTouched() {
    this.touched.emit();
    this.isTouched = true;
  }

  valueChanged(newValue: string) {
    this.notifyTouched();
    this.ngModelChange.emit(newValue);
  }
}
