import { Component, EventEmitter, Output } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { RemoteConfigsEndpoint } from 'src/app/main/dtos/settings/ob-settings.dto';

@Component({
  selector: 'app-create-remote-configs-endpoint',
  templateUrl: './create-remote-configs-endpoint.component.html',
  styleUrls: ['./create-remote-configs-endpoint.component.scss'],
})
export class CreateRemoteConfigsEndpointComponent {
  @Output() confirm = new EventEmitter<RemoteConfigsEndpoint>();

  faCircleQuestion = faCircleQuestion;
  url = '';
  apiKey = '';

  public reset() {
    this.url = '';
    this.apiKey = '';
  }

  submitForm() {
    this.confirm.emit({
      url: this.url,
      apiKey: this.apiKey,
    });
  }

  isFormValid() {
    return this.url.startsWith('http') && this.apiKey.length > 0;
  }
}
