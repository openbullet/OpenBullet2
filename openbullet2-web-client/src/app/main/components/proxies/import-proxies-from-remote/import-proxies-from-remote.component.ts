import { Component, EventEmitter, Output } from '@angular/core';
import { ProxyType } from 'src/app/main/enums/proxy-type';

export interface RemoteProxiesToImport {
  defaultType: ProxyType;
  defaultUsername: string;
  defaultPassword: string;
  url: string;
}

@Component({
  selector: 'app-import-proxies-from-remote',
  templateUrl: './import-proxies-from-remote.component.html',
  styleUrls: ['./import-proxies-from-remote.component.scss'],
})
export class ImportProxiesFromRemoteComponent {
  @Output() confirm = new EventEmitter<RemoteProxiesToImport>();
  url = '';
  defaultUsername = '';
  defaultPassword = '';
  defaultProxyType: ProxyType = ProxyType.Http;
  proxyTypes: ProxyType[] = [ProxyType.Http, ProxyType.Socks4, ProxyType.Socks4a, ProxyType.Socks5];

  public reset() {
    this.url = '';
    this.defaultUsername = '';
    this.defaultPassword = '';
    this.defaultProxyType = ProxyType.Http;
  }

  submitForm() {
    this.confirm.emit({
      defaultUsername: this.defaultUsername,
      defaultPassword: this.defaultPassword,
      defaultType: this.defaultProxyType,
      url: this.url,
    });
  }

  isFormValid() {
    return this.url.startsWith('http');
  }
}
