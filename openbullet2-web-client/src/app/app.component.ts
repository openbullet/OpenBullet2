import { Component } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { getBaseUrl } from './shared/utils/host';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [ConfirmationService]
})
export class AppComponent {
  title = 'OpenBullet 2';

  constructor() {
    const themeStyle = document.getElementById('app-theme') as any;

    if (themeStyle !== null) {
      themeStyle.href = getBaseUrl() + '/settings/theme';
    }
  }
}
