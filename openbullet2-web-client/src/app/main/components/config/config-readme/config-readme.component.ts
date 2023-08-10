import { Component } from '@angular/core';
import { faFileLines, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { ConfigService } from 'src/app/main/services/config.service';

@Component({
  selector: 'app-config-readme',
  templateUrl: './config-readme.component.html',
  styleUrls: ['./config-readme.component.scss']
})
export class ConfigReadmeComponent {
  config: ConfigDto | null = null;
  faTriangleExclamation = faTriangleExclamation;
  faFileLines = faFileLines;

  constructor(private configService: ConfigService) {
    this.configService.selectedConfig$
      .subscribe(config => this.config = config);
  }

  localSave() {
    if (this.config !== null) {
      this.configService.saveLocalConfig(this.config);
    }
  }

  valueChanged(event: Event) {
    if (this.config !== null) {
      this.config.readme = (event.target as HTMLInputElement).value;
      this.localSave();
    }
  }
}
