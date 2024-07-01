import { Component, EventEmitter, Output } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { ProxyCheckTarget } from 'src/app/main/dtos/settings/ob-settings.dto';

@Component({
  selector: 'app-create-proxy-check-target',
  templateUrl: './create-proxy-check-target.component.html',
  styleUrls: ['./create-proxy-check-target.component.scss'],
})
export class CreateProxyCheckTargetComponent {
  @Output() confirm = new EventEmitter<ProxyCheckTarget>();

  faCircleQuestion = faCircleQuestion;
  url = '';
  successKey = '';

  public reset() {
    this.url = '';
    this.successKey = '';
  }

  submitForm() {
    this.confirm.emit({
      url: this.url,
      successKey: this.successKey,
    });
  }

  isFormValid() {
    return this.url.startsWith('http') && this.successKey.length > 0;
  }
}
