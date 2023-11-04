import { Component, EventEmitter, Output } from '@angular/core';
import * as moment from 'moment';
import { ConfigInfoDto } from 'src/app/main/dtos/config/config-info.dto';
import { ConfigService } from 'src/app/main/services/config.service';

@Component({
  selector: 'app-select-config',
  templateUrl: './select-config.component.html',
  styleUrls: ['./select-config.component.scss']
})
export class SelectConfigComponent {
  @Output() confirm = new EventEmitter<ConfigInfoDto>();

  moment = moment;
  configs: ConfigInfoDto[] | null = null;
  filteredConfigs: ConfigInfoDto[] | null = null;
  searchTerm: string = '';

  constructor(
    private configService: ConfigService
  ) { }

  public refresh() {
    this.configs = null;
    this.configService.getAllConfigs(false).subscribe(configs => {
      this.configs = configs;
      this.filterConfigs();
    });
  }

  selectConfig(config: ConfigInfoDto) {
    this.confirm.emit(config);
  }

  searchBoxKeyDown(event: any) {
    if (event.key == 'Enter') {
      this.filterConfigs();
    }
  }

  filterConfigs() {
    if (this.configs === null) {
      return;
    }

    this.filteredConfigs = this.configs.filter(
      config => config.name.toLowerCase().includes(this.searchTerm.toLowerCase()));
  }
}
