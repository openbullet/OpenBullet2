import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { faCircleQuestion } from '@fortawesome/free-solid-svg-icons';
import { ProxyCheckTarget } from 'src/app/main/dtos/settings/ob-settings.dto';

@Component({
  selector: 'app-update-proxy-check-target',
  templateUrl: './update-proxy-check-target.component.html',
  styleUrls: ['./update-proxy-check-target.component.scss'],
})
export class UpdateProxyCheckTargetComponent implements OnChanges {
  @Input() target: ProxyCheckTarget | null = null;
  @Output() confirm = new EventEmitter<ProxyCheckTarget>();

  faCircleQuestion = faCircleQuestion;
  url = '';
  successKey = '';

  ngOnChanges(changes: SimpleChanges) {
    if (this.target === null) return;
    this.url = this.target.url;
    this.successKey = this.target.successKey;
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
