import { Component, EventEmitter, Output } from '@angular/core';
import { ProxyType } from 'src/app/main/enums/proxy-type';

export interface ProxiesToImport {
  defaultType: ProxyType;
  defaultUsername: string;
  defaultPassword: string;
  proxies: string[];
}

@Component({
  selector: 'app-import-proxies-from-text',
  templateUrl: './import-proxies-from-text.component.html',
  styleUrls: ['./import-proxies-from-text.component.scss'],
})
export class ImportProxiesFromTextComponent {
  @Output() confirm = new EventEmitter<ProxiesToImport>();
  proxies = '';
  defaultUsername = '';
  defaultPassword = '';
  defaultProxyType: ProxyType = ProxyType.Http;
  proxyTypes: ProxyType[] = [ProxyType.Http, ProxyType.Socks4, ProxyType.Socks4a, ProxyType.Socks5];

  public reset() {
    this.proxies = '';
    this.defaultUsername = '';
    this.defaultPassword = '';
    this.defaultProxyType = ProxyType.Http;
  }

  submitForm() {
    this.confirm.emit({
      defaultUsername: this.defaultUsername,
      defaultPassword: this.defaultPassword,
      defaultType: this.defaultProxyType,
      proxies: this.splitProxies(),
    });
  }

  isFormValid() {
    return this.splitProxies().length > 0;
  }

  splitProxies() {
    return this.proxies.split('\n').filter((p) => p.trim() !== '');
  }
}
