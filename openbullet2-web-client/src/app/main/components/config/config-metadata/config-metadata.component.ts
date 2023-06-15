import { Component, HostListener, ViewChild } from '@angular/core';
import { faPen, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { EditConfigImageComponent } from '../edit-config-image/edit-config-image.component';

@Component({
  selector: 'app-config-metadata',
  templateUrl: './config-metadata.component.html',
  styleUrls: ['./config-metadata.component.scss']
})
export class ConfigMetadataComponent {
  @ViewChild('editConfigImageComponent')
  editConfigImageComponent: EditConfigImageComponent | undefined = undefined;

  config: ConfigDto | null = null;
  faTriangleExclamation = faTriangleExclamation;
  faPen = faPen;
  editImageModalVisible = false;

  constructor(private configService: ConfigService) {
    this.configService.selectedConfig$
      .subscribe(config => this.config = config);
  }

  localSave() {
    if (this.config !== null) {
      this.configService.saveLocalConfig(this.config);
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
