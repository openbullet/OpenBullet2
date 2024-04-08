import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import { faInfo } from '@fortawesome/free-solid-svg-icons';
import { FileUpload } from 'primeng/fileupload';

@Component({
  selector: 'app-upload-configs',
  templateUrl: './upload-configs.component.html',
  styleUrls: ['./upload-configs.component.scss'],
})
export class UploadConfigsComponent {
  @Output() confirm = new EventEmitter<File[]>();
  @ViewChild('fileUpload') fileUpload: FileUpload | null = null;
  selectedFiles: File[] = [];
  isUploading = false;

  faInfo = faInfo;

  public reset() {
    this.fileUpload?.clear();
    this.isUploading = false;
  }

  // biome-ignore lint/suspicious/noExplicitAny: The signature of the original event is any.
  uploadError(event: any) {
    console.log(event);
  }

  canUpload() {
    return this.selectedFiles.length > 0 && !this.isUploading;
  }

  upload() {
    this.isUploading = true;
    this.confirm.emit(this.selectedFiles);
  }
}
