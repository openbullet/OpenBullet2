import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-contributor',
  templateUrl: './contributor.component.html',
  styleUrls: ['./contributor.component.scss'],
})
export class ContributorComponent {
  @Input() image: string | null = null;
  @Input() name = 'Name';
  @Input() role = 'Role';
}
