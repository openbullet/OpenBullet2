import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faCopy, faExclamationTriangle, faSave } from '@fortawesome/free-solid-svg-icons';
import { DebugService } from '../../services/debug.service';
import { saveFile } from 'src/app/shared/utils/files';
import { MessageService } from 'primeng/api';
import { ApiError } from 'src/app/shared/models/api-error';

// biome-ignore lint/suspicious/noExplicitAny: <explanation>
declare const Prism: any;

@Component({
  selector: 'app-error-details',
  templateUrl: './error-details.component.html',
  styleUrl: './error-details.component.scss',
  encapsulation: ViewEncapsulation.None
})
export class ErrorDetailsComponent implements OnInit {
  error: ApiError | null = null;
  faExclamationTriangle = faExclamationTriangle;
  faCopy = faCopy;
  faSave = faSave;

  constructor(private route: ActivatedRoute,
    private debugService: DebugService,
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      const messageKey = params['key'];
      const message = sessionStorage.getItem(messageKey);
      if (message !== null) {
        this.error = JSON.parse(message).data.error;

        setTimeout(() => {
          Prism.highlightAll();
        });
      }
    });
  }

  copyError() {
    if (this.error === null) return;
    navigator.clipboard.writeText(`Error code: ${this.error.errorCode}\nError message: ${this.error.message}\nError details: ${this.error.details}`);
    this.messageService.add({
      severity: 'info',
      summary: 'Copied to clipboard',
      detail: 'Error copied to clipboard'
    });
  }

  downloadLogFile() {
    this.debugService.downloadLogFile()
      .subscribe(resp => saveFile(resp));
  }
}
