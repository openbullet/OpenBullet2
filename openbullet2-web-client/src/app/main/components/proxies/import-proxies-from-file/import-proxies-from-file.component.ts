import { Component, EventEmitter, Output, ViewChild } from '@angular/core';
import { MessageService } from 'primeng/api';
import { FileUpload } from 'primeng/fileupload';
import { ProxyType } from 'src/app/main/enums/proxy-type';
import { ProxiesToImport } from '../import-proxies-from-text/import-proxies-from-text.component';

@Component({
  selector: 'app-import-proxies-from-file',
  templateUrl: './import-proxies-from-file.component.html',
  styleUrls: ['./import-proxies-from-file.component.scss'],
})
export class ImportProxiesFromFileComponent {
  @Output() confirm = new EventEmitter<ProxiesToImport>();
  @ViewChild('fileUpload') fileUpload: FileUpload | null = null;
  selectedFile: File | null = null;

  defaultUsername = '';
  defaultPassword = '';
  defaultProxyType: ProxyType = ProxyType.Http;
  proxyTypes: ProxyType[] = [ProxyType.Http, ProxyType.Socks4, ProxyType.Socks4a, ProxyType.Socks5];

  constructor(private messageService: MessageService) { }

  public reset() {
    this.fileUpload?.clear();
    this.selectedFile = null;
    this.defaultUsername = '';
    this.defaultPassword = '';
    this.defaultProxyType = ProxyType.Http;
  }

  submitForm() {
    if (this.selectedFile === null) {
      console.log('No files selected');
      return;
    }

    const fileReader = new FileReader();

    fileReader.onload = (e) => {
      if (fileReader.result === null) {
        this.messageService.add({
          severity: 'error',
          summary: 'Invalid file',
          detail: `Could not read lines from file ${this.selectedFile?.name}`,
        });
        return;
      }

      const lines = fileReader.result.toString().split(/[\r\n]+/);

      this.confirm.emit({
        defaultUsername: this.defaultUsername,
        defaultPassword: this.defaultPassword,
        defaultType: this.defaultProxyType,
        proxies: lines,
      });
    };

    fileReader.readAsText(this.selectedFile);
  }

  isFormValid() {
    return this.selectedFile !== null;
  }

  readProxiesFromFile() {
    this.selectedFile;
  }
}
