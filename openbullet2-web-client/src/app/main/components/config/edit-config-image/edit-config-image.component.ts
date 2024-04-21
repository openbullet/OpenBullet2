import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import { FileUpload } from 'primeng/fileupload';
import { ConfigService } from 'src/app/main/services/config.service';
import { base64ArrayBuffer } from 'src/app/shared/utils/base64ArrayBuffer';

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

  constructor(private configService: ConfigService) { }

  setImage(base64Image: string) {
    this.base64Image = base64Image;
    this.selectedFile = null;
    this.remoteUrl = '';
  }

  downloadImage() {
    this.configService.getRemoteImage(this.remoteUrl)
      .subscribe((blob) => this.setImageFromBlob(blob));
  }

  setImageFromBlob(blob: Blob) {
    const reader = new FileReader();
    reader.onloadend = () => {
      this.base64Image = base64ArrayBuffer(reader.result as ArrayBuffer);
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
