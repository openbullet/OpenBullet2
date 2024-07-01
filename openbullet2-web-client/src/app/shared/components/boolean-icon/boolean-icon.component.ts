import { Component, Input } from '@angular/core';
import { faCheck, faXmark } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-boolean-icon',
  templateUrl: './boolean-icon.component.html',
  styleUrls: ['./boolean-icon.component.scss'],
})
export class BooleanIconComponent {
  @Input() value = false;
  @Input() useColors = true;

  faCheck = faCheck;
  faXMark = faXmark;
}
