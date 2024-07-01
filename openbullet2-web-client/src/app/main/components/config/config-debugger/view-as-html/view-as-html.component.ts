import { Component, Input, OnChanges, SimpleChanges, ViewEncapsulation } from '@angular/core';
import { faCode, faCopy, faDownload, faEye } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';

// biome-ignore lint/suspicious/noExplicitAny: <explanation>
declare const Prism: any;

@Component({
  selector: 'app-view-as-html',
  templateUrl: './view-as-html.component.html',
  styleUrls: ['./view-as-html.component.scss']
})
export class ViewAsHtmlComponent implements OnChanges {
  @Input() html = '';

  faCode = faCode;
  faCopy = faCopy;
  faDownload = faDownload;
  faEye = faEye;

  showRawHtml = false;
  isJson = false;

  // TODO: We need to rework and improve this component

  constructor(private messageService: MessageService) { }

  ngOnChanges(changes: SimpleChanges): void {
    // If html starts with '{' and ends with '}', it's probably JSON
    this.isJson = this.html.trim().startsWith('{') && this.html.trim().endsWith('}');

    if (this.isJson) {
      setTimeout(() => {
        Prism.highlightAll();
      });
    }
  }

  toggleShowRawHtml() {
    this.showRawHtml = !this.showRawHtml;

    if (this.showRawHtml) {
      setTimeout(() => {
        Prism.highlightAll();
      });
    }
  }

  copySource() {
    navigator.clipboard.writeText(this.html);
    this.messageService.add({
      severity: 'info',
      summary: 'Copied to clipboard',
      detail: `${this.isJson ? 'JSON' : 'HTML'} copied to clipboard`
    });
  }

  downloadSource() {
    const blob = new Blob([this.html], { type: 'text/html' });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = this.isJson ? 'source.json' : 'source.html';
    a.click();

    URL.revokeObjectURL(url);
  }
}
