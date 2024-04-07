import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DiscordWebhookHitOutput } from 'src/app/main/dtos/job/multi-run-job-options.dto';

@Component({
  selector: 'app-configure-discord',
  templateUrl: './configure-discord.component.html',
  styleUrls: ['./configure-discord.component.scss'],
})
export class ConfigureDiscordComponent {
  @Output() confirm = new EventEmitter<void>();

  hitOutput: DiscordWebhookHitOutput | null = null;
  webhook = '';
  username = '';
  avatarUrl = '';
  onlyHits = false;

  public setHitOutput(hitOutput: DiscordWebhookHitOutput) {
    this.hitOutput = hitOutput;
    this.webhook = hitOutput.webhook;
    this.username = hitOutput.username;
    this.avatarUrl = hitOutput.avatarUrl;
    this.onlyHits = hitOutput.onlyHits;
  }

  submitForm() {
    if (this.hitOutput === null) {
      console.log('Hit output is null, this should not happen!');
      return;
    }

    this.hitOutput.webhook = this.webhook;
    this.hitOutput.username = this.username;
    this.hitOutput.avatarUrl = this.avatarUrl;
    this.hitOutput.onlyHits = this.onlyHits;

    this.confirm.emit();
  }

  isFormValid() {
    return this.webhook.startsWith('https') && this.avatarUrl.startsWith('https');
  }
}
