import { Component, EventEmitter, Output } from '@angular/core';
import { TelegramBotHitOutput } from 'src/app/main/dtos/job/multi-run-job-options.dto';

@Component({
  selector: 'app-configure-telegram',
  templateUrl: './configure-telegram.component.html',
  styleUrls: ['./configure-telegram.component.scss'],
})
export class ConfigureTelegramComponent {
  @Output() confirm = new EventEmitter<void>();

  hitOutput: TelegramBotHitOutput | null = null;
  apiServer = '';
  token = '';
  chatId = 0;
  onlyHits = false;

  public setHitOutput(hitOutput: TelegramBotHitOutput) {
    this.hitOutput = hitOutput;
    this.apiServer = hitOutput.apiServer;
    this.token = hitOutput.token;
    this.chatId = hitOutput.chatId;
    this.onlyHits = hitOutput.onlyHits;
  }

  submitForm() {
    if (this.hitOutput === null) {
      console.log('Hit output is null, this should not happen!');
      return;
    }

    this.hitOutput.apiServer = this.apiServer;
    this.hitOutput.token = this.token;
    this.hitOutput.chatId = this.chatId;
    this.hitOutput.onlyHits = this.onlyHits;

    this.confirm.emit();
  }

  isFormValid() {
    return this.chatId !== 0;
  }
}
