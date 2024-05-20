import { Component, Input } from '@angular/core';
import { faQuestionCircle } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-docs-button',
  templateUrl: './docs-button.component.html',
  styleUrl: './docs-button.component.scss'
})
export class DocsButtonComponent {
  @Input() path = 'intro';
  @Input() class = '';
  @Input() buttonClass = '';

  faQuestionCircle = faQuestionCircle;
}
