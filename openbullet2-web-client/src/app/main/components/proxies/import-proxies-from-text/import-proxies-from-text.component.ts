import { Component, EventEmitter, Output } from '@angular/core';

export interface ProxiesToImport {
  defaultType: string;
  defaultUsername: string;
  defaultPassword: string;
  proxies: string[]
}

@Component({
  selector: 'app-import-proxies-from-text',
  templateUrl: './import-proxies-from-text.component.html',
  styleUrls: ['./import-proxies-from-text.component.scss']
})
export class ImportProxiesFromTextComponent {
  @Output() confirm = new EventEmitter<ProxiesToImport>();
  proxies: string = '';
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
    this.proxies = '';
    this.defaultUsername = '';
    this.defaultPassword = '';
    this.defaultProxyType = 'http';
  }

  submitForm() {
    this.confirm.emit({
      defaultUsername: this.defaultUsername,
      defaultPassword: this.defaultPassword,
      defaultType: this.defaultProxyType,
      proxies: this.splitProxies()
    });
  }

  isFormValid() {
    return this.splitProxies().length > 0;
  }

  splitProxies() {
    return this.proxies.split('\n').filter(p => p.trim() !== '');
  }
}
