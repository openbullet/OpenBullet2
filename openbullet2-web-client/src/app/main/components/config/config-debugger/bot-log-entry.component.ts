import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { faWindowMaximize } from '@fortawesome/free-solid-svg-icons';
import { BotLoggerEntry } from 'src/app/main/models/config-debugger-settings';

@Component({
  selector: 'app-bot-log-entry',
  templateUrl: './bot-log-entry.component.html',
  styleUrls: ['./bot-log-entry.component.scss'],
})
export class BotLogEntryComponent implements OnChanges {
  @Input() log!: BotLoggerEntry;
  @Output() viewAsHtml = new EventEmitter<BotLoggerEntry>();

  readonly collapsedLogLength = 1000;

  faWindowMaximize = faWindowMaximize;
  isExpanded = false;
  isLongMessage = false;
  collapsedMessage = '';

  ngOnChanges(): void {
    this.isExpanded = false;
    this.isLongMessage = this.log.message.length > this.collapsedLogLength;
    this.collapsedMessage = this.isLongMessage
      ? this.log.message.substring(0, this.collapsedLogLength)
      : this.log.message;
  }

  expand() {
    this.isExpanded = true;
  }

  collapse() {
    this.isExpanded = false;
  }

  openHtmlViewer() {
    this.viewAsHtml.emit(this.log);
  }
}
