import { HttpEventType } from '@angular/common/http';
import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FileSelectEvent, FileUpload } from 'primeng/fileupload';
import { catchError, map, throwError } from 'rxjs';
import { CreateWordlistDto } from 'src/app/main/dtos/wordlist/create-wordlist.dto';
import { WordlistFileDto } from 'src/app/main/dtos/wordlist/wordlist-file.dto';
import { WordlistService } from 'src/app/main/services/wordlist.service';

@Component({
  selector: 'app-upload-wordlist',
  templateUrl: './upload-wordlist.component.html',
  styleUrls: ['./upload-wordlist.component.scss'],
})
export class UploadWordlistComponent {
  @Input() wordlistTypes: string[] = ['Default'];
  @Output() confirm = new EventEmitter<CreateWordlistDto>();
  @ViewChild('fileUpload') fileUpload: FileUpload | null = null;
  selectedFile: File | null = null;
  name = '';
  purpose = '';
  wordlistType = 'Default';
  filePath: string | null = null; // The path of the file on the server
  isUploading = false;
  uploadProgress: number | null = null;

  constructor(
    private wordlistService: WordlistService,
    private messageService: MessageService,
  ) { }

  public reset() {
    this.name = '';
    this.purpose = '';
    this.wordlistType = 'Default';
    this.filePath = null;
    this.fileUpload?.clear();
    this.selectedFile = null;
    this.uploadProgress = null;
  }

  canUpload() {
    return this.selectedFile !== null;
  }

  upload() {
    if (this.selectedFile === null) {
      console.log('No files selected');
      return;
    }

    this.isUploading = true;

    this.wordlistService
      .uploadWordlistFile(this.selectedFile)
      .pipe(
        // biome-ignore lint/suspicious/noExplicitAny: any
        map((event: any) => {
          if (event.type === HttpEventType.UploadProgress) {
            this.uploadProgress = Math.round((100 / event.total) * event.loaded);
          } else if (event.type === HttpEventType.Response) {
            const resp: WordlistFileDto = event.body;
            this.uploadProgress = 100;
            this.messageService.add({
              severity: 'success',
              summary: 'Uploaded',
              detail: `Wordlist file uploaded to ${resp.filePath}`,
            });
            this.filePath = resp.filePath;
            this.isUploading = false;
          }
        }),
        // biome-ignore lint/suspicious/noExplicitAny: any
        catchError((err: any) => {
          this.uploadProgress = null;
          console.log(err.message);
          return throwError(() => err.message);
        }),
      )
      .subscribe();
  }

  // biome-ignore lint/suspicious/noExplicitAny: any
  uploadError(event: any) {
    console.log(event);
  }

  clearUpload() {
    this.filePath = null;
    this.selectedFile = null;
    this.uploadProgress = null;
  }

  fileSelected(event: FileSelectEvent) {
    if (this.name.length === 0) {
      // Remove the extension from the file name
      this.name = event.files[0].name.replace(/\.[^/.]+$/, '');
    }
  }

  submitForm() {
    if (this.filePath === null) {
      console.log('File path is null, this should not happen!');
      return;
    }

    this.confirm.emit({
      name: this.name,
      purpose: this.purpose,
      wordlistType: this.wordlistType,
      filePath: this.filePath,
    });
  }

  isFormValid() {
    return this.name.length > 0 && this.filePath !== null;
  }
}
