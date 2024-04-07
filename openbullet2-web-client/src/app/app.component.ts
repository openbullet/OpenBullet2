import { Component } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { applyAppTheme } from './shared/utils/theme';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  providers: [ConfirmationService],
})
export class AppComponent {
  title = 'OpenBullet 2';

  constructor() {
    applyAppTheme();
  }
}
