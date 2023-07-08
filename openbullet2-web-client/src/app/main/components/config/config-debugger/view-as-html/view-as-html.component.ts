import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-view-as-html',
  templateUrl: './view-as-html.component.html',
  styleUrls: ['./view-as-html.component.scss']
})
export class ViewAsHtmlComponent implements OnChanges {
  @Input() html: string = '';

  sanitizedHtml: string = '';

  constructor(private domSanitizer: DomSanitizer) {}

  ngOnChanges(changes: SimpleChanges) {
    // If I bind the SafeHtml directly the modal breaks for some reason
    this.sanitizedHtml = (this.domSanitizer.bypassSecurityTrustHtml(this.html) as any)
    .changingThisBreaksApplicationSecurity;
  }
}
