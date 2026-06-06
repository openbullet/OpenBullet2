import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import { faPaste } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
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
  faPaste = faPaste;

  constructor(
    private configService: ConfigService,
    private messageService: MessageService,
  ) { }

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

  async pasteImageFromClipboard() {
    if (!navigator.clipboard?.read) {
      this.messageService.add({
        severity: 'error',
        summary: 'Clipboard unavailable',
        detail: 'Image paste is not supported by this browser or context',
      });
      return;
    }

    try {
      const items = await navigator.clipboard.read();
      const imageItem = items.find((item) => item.types.some((type) => type.startsWith('image/')));

      if (!imageItem) {
        this.messageService.add({
          severity: 'warn',
          summary: 'No image found',
          detail: 'The clipboard does not contain an image',
        });
        return;
      }

      const imageType = imageItem.types.find((type) => type.startsWith('image/'));

      if (!imageType) {
        this.messageService.add({
          severity: 'warn',
          summary: 'No image found',
          detail: 'The clipboard does not contain an image',
        });
        return;
      }

      const blob = await imageItem.getType(imageType);
      this.setImageFromBlob(blob);
    } catch {
      this.messageService.add({
        severity: 'error',
        summary: 'Paste failed',
        detail: 'The image could not be read from the clipboard',
      });
    }
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
