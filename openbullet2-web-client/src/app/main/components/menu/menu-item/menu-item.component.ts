import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { IconDefinition } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-menu-item',
  templateUrl: './menu-item.component.html',
  styleUrls: ['./menu-item.component.scss'],
})
export class MenuItemComponent {
  @Input() label!: string;
  @Input() link!: string;
  @Input() icon!: IconDefinition;
  @Input() active!: boolean;

  constructor(private router: Router) {}

  getItemClass(): string {
    return this.active ? 'menu-item selected' : 'menu-item';
  }

  navigate(e: MouseEvent): void {
    // If ctrl + left click, open in new tab
    if (e.ctrlKey) {
      window.open(this.link, '_blank');
    } else {
      this.router.navigate([this.link]);
    }
  }
}
