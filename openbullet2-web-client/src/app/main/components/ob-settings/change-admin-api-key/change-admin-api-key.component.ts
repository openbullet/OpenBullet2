import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-change-admin-api-key',
  templateUrl: './change-admin-api-key.component.html',
  styleUrls: ['./change-admin-api-key.component.scss'],
})
export class ChangeAdminApiKeyComponent {
  @Output() confirm = new EventEmitter<string>();

  apiKey = '';

  public reset(apiKey: string) {
    this.apiKey = apiKey;
  }

  submitForm() {
    this.confirm.emit(this.apiKey);
  }

  isFormValid() {
    return this.apiKey.length === 0 || this.apiKey.length >= 12;
  }

  generateApiKey() {
    this.apiKey = Array.from({ length: 32 }, () => Math.floor(Math.random() * 36).toString(36)).join('');
  }
}
