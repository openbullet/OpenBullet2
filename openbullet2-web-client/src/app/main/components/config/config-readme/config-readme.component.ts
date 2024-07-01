import { Component, HostListener } from '@angular/core';
import { faFileLines, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { ConfigService } from 'src/app/main/services/config.service';

@Component({
  selector: 'app-config-readme',
  templateUrl: './config-readme.component.html',
  styleUrls: ['./config-readme.component.scss'],
})
export class ConfigReadmeComponent {
  // Listen for CTRL+S on the page
  @HostListener('document:keydown.control.s', ['$event'])
  onKeydownHandler(event: KeyboardEvent) {
    event.preventDefault();

    if (this.config !== null) {
      this.configService.saveConfig(this.config, true).subscribe((c) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Saved',
          detail: `${c.metadata.name} was saved`,
        });
      });
    }
  }

  config: ConfigDto | null = null;
  faTriangleExclamation = faTriangleExclamation;
  faFileLines = faFileLines;

  constructor(
    private configService: ConfigService,
    private messageService: MessageService,
  ) {
    this.configService.selectedConfig$.subscribe((config) => {
      this.config = config;
    });
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
