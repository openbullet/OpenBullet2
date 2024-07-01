import { Component, HostListener, ViewChild } from '@angular/core';
import { faPen, faTags, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { EditConfigImageComponent } from '../edit-config-image/edit-config-image.component';

@Component({
  selector: 'app-config-metadata',
  templateUrl: './config-metadata.component.html',
  styleUrls: ['./config-metadata.component.scss'],
})
export class ConfigMetadataComponent {
  @ViewChild('editConfigImageComponent')
  editConfigImageComponent: EditConfigImageComponent | undefined = undefined;

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
  faPen = faPen;
  faTags = faTags;
  editImageModalVisible = false;

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

  nameChanged() {
    if (this.config !== null) {
      this.configService.nameChanged(this.config?.metadata.name);
    }
  }

  openEditImageModal() {
    if (this.config !== null) {
      this.editConfigImageComponent?.setImage(this.config.metadata.base64Image);
      this.editImageModalVisible = true;
    }
  }

  editImage(base64Image: string) {
    if (this.config !== null) {
      this.config.metadata.base64Image = base64Image;
      this.localSave();
      this.editImageModalVisible = false;
    }
  }
}
