import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import { FileUpload } from 'primeng/fileupload';

@Component({
  selector: 'app-add-plugin',
  templateUrl: './add-plugin.component.html',
  styleUrls: ['./add-plugin.component.scss'],
})
export class AddPluginComponent {
  @Output() confirm = new EventEmitter<File>();
  @ViewChild('fileUpload') fileUpload: FileUpload | null = null;
  selectedFile: File | null = null;

  public reset() {
    this.fileUpload?.clear();
    this.selectedFile = null;
  }

  submitForm() {
    if (this.selectedFile === null) {
      console.log('No files selected');
      return;
    }

    this.confirm.emit(this.selectedFile);
  }

  isFormValid() {
    return this.selectedFile !== null;
  }
}
