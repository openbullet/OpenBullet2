import { Component } from '@angular/core';
import { faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-info',
  templateUrl: './info.component.html',
  styleUrls: ['./info.component.scss']
})
export class InfoComponent {
  faTriangleExclamation = faTriangleExclamation;
}
