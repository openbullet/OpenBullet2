import { Component, EventEmitter, Output } from '@angular/core';

export interface RemoteProxiesToImport {
  defaultType: string;
  defaultUsername: string;
  defaultPassword: string;
  url: string
}

@Component({
  selector: 'app-import-proxies-from-remote',
  templateUrl: './import-proxies-from-remote.component.html',
  styleUrls: ['./import-proxies-from-remote.component.scss']
})
export class ImportProxiesFromRemoteComponent {
  @Output() confirm = new EventEmitter<RemoteProxiesToImport>();
  url: string = '';
  defaultUsername: string = '';
  defaultPassword: string = '';
  defaultProxyType: string = '';
  proxyTypes: string[] = [
    'http',
    'socks4',
    'socks5',
    'socks4a'
  ];

  public reset() {
    this.url = '';
    this.defaultUsername = '';
    this.defaultPassword = '';
    this.defaultProxyType = 'http';
  }

  submitForm() {
    this.confirm.emit({
      defaultUsername: this.defaultUsername,
      defaultPassword: this.defaultPassword,
      defaultType: this.defaultProxyType,
      url: this.url
    });
  }

  isFormValid() {
    return this.url.startsWith('http');
  }
}
