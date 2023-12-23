import { Component, OnInit } from '@angular/core';
import { faGripLines, faTriangleExclamation } from '@fortawesome/free-solid-svg-icons';
import { BlockDescriptors } from 'src/app/main/dtos/config/block-descriptor.dto';
import { ConfigDto } from 'src/app/main/dtos/config/config.dto';
import { ConfigService } from 'src/app/main/services/config.service';

@Component({
  selector: 'app-config-stacker',
  templateUrl: './config-stacker.component.html',
  styleUrls: ['./config-stacker.component.scss']
})
export class ConfigStackerComponent implements OnInit {
  config: ConfigDto | null = null;
  stack: any[] | null = null;
  descriptors: BlockDescriptors | null = null;

  faGripLines = faGripLines;
  faTriangleExclamation = faTriangleExclamation;

  constructor(private configService: ConfigService) {
    this.configService.selectedConfig$
      .subscribe(config => this.config = config);
  }

  ngOnInit(): void {
    if (this.config === null) {
      return;
    }

    this.configService.convertLoliCodeToStack(this.config.loliCodeScript)
      .subscribe(resp => {
        console.log(resp);
        this.stack = resp.stack;
      });

    this.configService.getBlockDescriptors()
      .subscribe(descriptors => this.descriptors = descriptors);
  }
}
