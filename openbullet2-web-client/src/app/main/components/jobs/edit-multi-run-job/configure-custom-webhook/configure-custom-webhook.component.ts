import { Component, EventEmitter, Output } from '@angular/core';
import { CustomWebhookHitOutput } from 'src/app/main/dtos/job/multi-run-job-options.dto';

@Component({
  selector: 'app-configure-custom-webhook',
  templateUrl: './configure-custom-webhook.component.html',
  styleUrls: ['./configure-custom-webhook.component.scss'],
})
export class ConfigureCustomWebhookComponent {
  @Output() confirm = new EventEmitter<void>();

  hitOutput: CustomWebhookHitOutput | null = null;
  url = '';
  user = '';
  onlyHits = false;

  public setHitOutput(hitOutput: CustomWebhookHitOutput) {
    this.hitOutput = hitOutput;
    this.url = hitOutput.url;
    this.user = hitOutput.user;
    this.onlyHits = hitOutput.onlyHits;
  }

  submitForm() {
    if (this.hitOutput === null) {
      console.log('Hit output is null, this should not happen!');
      return;
    }

    this.hitOutput.url = this.url;
    this.hitOutput.user = this.user;
    this.hitOutput.onlyHits = this.onlyHits;

    this.confirm.emit();
  }

  isFormValid() {
    return this.url.startsWith('https');
  }
}
