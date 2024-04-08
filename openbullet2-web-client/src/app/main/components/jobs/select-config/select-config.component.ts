import { Component, EventEmitter, Output } from '@angular/core';
import * as moment from 'moment';
import { ConfigInfoDto } from 'src/app/main/dtos/config/config-info.dto';
import { ConfigService } from 'src/app/main/services/config.service';

@Component({
  selector: 'app-select-config',
  templateUrl: './select-config.component.html',
  styleUrls: ['./select-config.component.scss'],
})
export class SelectConfigComponent {
  @Output() confirm = new EventEmitter<ConfigInfoDto>();

  moment = moment;
  configs: ConfigInfoDto[] | null = null;
  filteredConfigs: ConfigInfoDto[] | null = null;
  searchTerm = '';
  selectedConfig: ConfigInfoDto | null = null;
  readme: string | null = null;

  constructor(private configService: ConfigService) {}

  public refresh() {
    this.configs = null;
    this.configService.getAllConfigs(false).subscribe((configs) => {
      this.configs = configs;
      this.filterConfigs();
    });
  }

  selectConfig(config: ConfigInfoDto) {
    if (config === this.selectedConfig) {
      return;
    }

    this.readme = null;
    this.selectedConfig = config;
    this.configService.getReadme(config.id).subscribe((readme) => {
      this.readme = readme.markdownText;
    });
  }

  chooseConfig(config: ConfigInfoDto) {
    this.confirm.emit(config);
  }

  searchBoxKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter') {
      this.filterConfigs();
    }
  }

  filterConfigs() {
    if (this.configs === null) {
      return;
    }

    this.filteredConfigs = this.configs.filter((config) =>
      config.name.toLowerCase().includes(this.searchTerm.toLowerCase()),
    );
  }
}
