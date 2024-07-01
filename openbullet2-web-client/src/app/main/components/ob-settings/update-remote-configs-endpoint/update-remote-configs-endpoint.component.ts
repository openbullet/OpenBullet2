import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { RemoteConfigsEndpoint } from 'src/app/main/dtos/settings/ob-settings.dto';

@Component({
  selector: 'app-update-remote-configs-endpoint',
  templateUrl: './update-remote-configs-endpoint.component.html',
  styleUrls: ['./update-remote-configs-endpoint.component.scss'],
})
export class UpdateRemoteConfigsEndpointComponent implements OnChanges {
  @Input() endpoint: RemoteConfigsEndpoint | null = null;
  @Output() confirm = new EventEmitter<RemoteConfigsEndpoint>();

  faCircleQuestion = faCircleQuestion;
  url = '';
  apiKey = '';

  ngOnChanges(changes: SimpleChanges) {
    if (this.endpoint === null) return;
    this.url = this.endpoint.url;
    this.apiKey = this.endpoint.apiKey;
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
