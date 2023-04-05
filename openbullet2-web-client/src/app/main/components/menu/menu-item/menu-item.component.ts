import { Component, Input } from '@angular/core';
import { IconDefinition } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-menu-item',
  templateUrl: './menu-item.component.html',
  styleUrls: ['./menu-item.component.scss']
})
export class MenuItemComponent {
  @Input() label!: string;
  @Input() link!: string;
  @Input() icon!: IconDefinition;
  @Input() active!: boolean;

  getItemClass(): string {
    return this.active ? 'menu-item selected' : 'menu-item';
  }
}
