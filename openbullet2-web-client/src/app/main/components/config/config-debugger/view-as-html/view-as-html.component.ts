import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
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
  prettyPrintJson = true;
  highlightedSource = '';
  private parsedJson: unknown = null;

  // TODO: We need to rework and improve this component

  constructor(private messageService: MessageService) { }

  ngOnChanges(changes: SimpleChanges): void {
    this.parsedJson = null;

    try {
      this.parsedJson = JSON.parse(this.html);
      this.isJson = true;
    } catch {
      this.isJson = false;
    }

    this.updateHighlightedSource();
  }

  toggleShowRawHtml() {
    this.showRawHtml = !this.showRawHtml;
    this.updateHighlightedSource();
  }

  togglePrettyPrintJson() {
    this.updateHighlightedSource();
  }

  copySource() {
    navigator.clipboard.writeText(this.getDisplayedSource());
    this.messageService.add({
      severity: 'info',
      summary: 'Copied to clipboard',
      detail: `${this.isJson ? 'JSON' : 'HTML'} copied to clipboard`
    });
  }

  downloadSource() {
    const blob = new Blob([this.getDisplayedSource()], {
      type: this.isJson ? 'application/json' : 'text/html'
    });
    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');
    a.href = url;
    a.download = this.isJson ? 'source.json' : 'source.html';
    a.click();

    URL.revokeObjectURL(url);
  }

  private getDisplayedSource(): string {
    if (!this.isJson || !this.prettyPrintJson) {
      return this.html;
    }

    return JSON.stringify(this.parsedJson, null, 2);
  }

  private updateHighlightedSource() {
    if (this.isJson) {
      this.highlightedSource = Prism.highlight(
        this.getDisplayedSource(),
        Prism.languages.json,
        'json'
      );

      return;
    }

    if (this.showRawHtml) {
      this.highlightedSource = Prism.highlight(
        this.html,
        Prism.languages.markup,
        'markup'
      );

      return;
    }

    this.highlightedSource = '';
  }
}
