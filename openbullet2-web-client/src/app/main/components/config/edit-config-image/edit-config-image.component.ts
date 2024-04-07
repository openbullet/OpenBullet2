import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import { FileUpload } from 'primeng/fileupload';

@Component({
  selector: 'app-edit-config-image',
  templateUrl: './edit-config-image.component.html',
  styleUrls: ['./edit-config-image.component.scss'],
})
export class EditConfigImageComponent {
  @Output() confirm = new EventEmitter<string>();
  @ViewChild('fileUpload') fileUpload: FileUpload | null = null;
  selectedFile: File | null = null;
  base64Image: string | null = null;
  remoteUrl = '';

  constructor(private http: HttpClient) { }

  setImage(base64Image: string) {
    this.base64Image = base64Image;
    this.selectedFile = null;
    this.remoteUrl = '';
  }

  downloadImage() {
    this.http
      .get<Blob>(this.remoteUrl, {
        responseType: 'blob' as 'json',
      })
      .subscribe((blob) => this.setImageFromBlob(blob));
  }

  setImageFromBlob(blob: Blob) {
    const reader = new FileReader();
    reader.onloadend = () => {
      this.base64Image = window.btoa(reader.result as string);
    };
    reader.readAsArrayBuffer(blob);
  }

  submitForm() {
    if (this.base64Image !== null) {
      this.confirm.emit(this.base64Image);
    }
  }

  isFormValid() {
    return this.base64Image !== null;
  }
}
