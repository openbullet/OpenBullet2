import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

/** usage
 * [innerHTML]="value | safeHtml"
 */
@Pipe({ name: 'safeHtml' })
export class SafeHtmlPipe implements PipeTransform {
    constructor(private sanitizer: DomSanitizer) { }

    transform(html: string): SafeHtml {
        // biome-ignore lint/suspicious/noExplicitAny: it doesn't work without any
        const result = this.sanitizer.bypassSecurityTrustHtml(html) as any;
        return result.changingThisBreaksApplicationSecurity;
    }
}
