import { Component, ViewEncapsulation } from '@angular/core';
import { ConfirmationService, Message } from 'primeng/api';
import { applyAppTheme } from './shared/utils/theme';
import { faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [ConfirmationService],
  encapsulation: ViewEncapsulation.None,
})
export class AppComponent {
  title = 'OpenBullet 2';

  faExclamationTriangle = faExclamationTriangle;

  constructor(private router: Router) {
    applyAppTheme();
  }

  openErrorDetails(message: Message): void {
    // Random key to store the message in sessionStorage
    const messageKey = `error-${Math.random().toString(36).substring(7)}`;
    sessionStorage.setItem(messageKey, JSON.stringify(message));
    const url = this.router.createUrlTree(['/error-details'], { queryParams: { key: messageKey } });
    window.open(this.router.serializeUrl(url), '_blank');
  }
}
