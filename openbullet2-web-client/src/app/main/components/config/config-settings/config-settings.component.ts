import { Component, HostListener, OnInit } from '@angular/core';
import { faPlus, faTriangleExclamation, faWrench, faX } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import {
  ConfigDto,
  CustomInputDto,
  LinesFromFileResourceDto,
  RandomLinesFromFileResourceDto,
  RegexDataRuleDto,
  SimpleDataRuleDto,
  StringRule,
} from 'src/app/main/dtos/config/config.dto';
import { EnvironmentSettingsDto } from 'src/app/main/dtos/settings/environment-settings.dto';
import { ProxyType } from 'src/app/main/enums/proxy-type';
import { ConfigService } from 'src/app/main/services/config.service';
import { SettingsService } from 'src/app/main/services/settings.service';

@Component({
  selector: 'app-config-settings',
  templateUrl: './config-settings.component.html',
  styleUrls: ['./config-settings.component.scss'],
})
export class ConfigSettingsComponent implements OnInit {
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

  envSettings: EnvironmentSettingsDto | null = null;
  config: ConfigDto | null = null;
  faTriangleExclamation = faTriangleExclamation;
  faPlus = faPlus;
  faX = faX;
  faWrench = faWrench;
  editImageModalVisible = false;

  botStatuses: string[] = [];
  proxyTypes: ProxyType[] = [ProxyType.Http, ProxyType.Socks4, ProxyType.Socks4a, ProxyType.Socks5];
  wordlistTypes: string[] = [];
  stringRules: StringRule[] = [
    StringRule.EqualTo,
    StringRule.Contains,
    StringRule.LongerThan,
    StringRule.ShorterThan,
    StringRule.ContainsAny,
    StringRule.ContainsAll,
    StringRule.StartsWith,
    StringRule.EndsWith,
  ];

  constructor(
    private configService: ConfigService,
    private settingsService: SettingsService,
    private messageService: MessageService,
  ) {
    this.configService.selectedConfig$.subscribe((config) => {
      this.config = config;
    });
  }

  ngOnInit(): void {
    this.settingsService.getEnvironmentSettings().subscribe((envSettings) => {
      this.envSettings = envSettings;
      this.botStatuses = [
        'SUCCESS',
        'NONE',
        'FAIL',
        'RETRY',
        'BAN',
        'ERROR',
        ...envSettings.customStatuses.map((s) => s.name),
      ];
      this.wordlistTypes = envSettings.wordlistTypes.map((w) => w.name);
    });
  }

  localSave() {
    if (this.config !== null) {
      this.configService.saveLocalConfig(this.config);
    }
  }

  createSimpleDataRule() {
    if (this.config !== null) {
      this.config.settings.dataSettings.dataRules.simple = [
        ...this.config.settings.dataSettings.dataRules.simple,
        {
          sliceName: '',
          invert: false,
          comparison: StringRule.EqualTo,
          stringToCompare: '',
          caseSensitive: true,
        },
      ];
      this.localSave();
    }
  }

  createRegexDataRule() {
    if (this.config !== null) {
      this.config.settings.dataSettings.dataRules.regex = [
        ...this.config.settings.dataSettings.dataRules.regex,
        {
          sliceName: '',
          regexToMatch: '^.*$',
          invert: false,
        },
      ];
      this.localSave();
    }
  }

  createLinesFromFileResource() {
    if (this.config !== null) {
      this.config.settings.dataSettings.resources.linesFromFile = [
        ...this.config.settings.dataSettings.resources.linesFromFile,
        {
          name: 'resource',
          location: 'resource.txt',
          loopsAround: true,
          ignoreEmptyLines: true,
        },
      ];
      this.localSave();
    }
  }

  createRandomLinesFromFileResource() {
    if (this.config !== null) {
      this.config.settings.dataSettings.resources.randomLinesFromFile = [
        ...this.config.settings.dataSettings.resources.randomLinesFromFile,
        {
          name: 'resource',
          location: 'resource.txt',
          unique: false,
          ignoreEmptyLines: true,
        },
      ];
      this.localSave();
    }
  }

  createCustomInput() {
    if (this.config !== null) {
      this.config.settings.inputSettings.customInputs = [
        ...this.config.settings.inputSettings.customInputs,
        {
          description: '',
          variableName: '',
          defaultAnswer: '',
        },
      ];
      this.localSave();
    }
  }

  removeSimpleDataRule(rule: SimpleDataRuleDto) {
    if (this.config !== null) {
      const index = this.config.settings.dataSettings.dataRules.simple.indexOf(rule);
      if (index !== -1) {
        this.config.settings.dataSettings.dataRules.simple.splice(index, 1);
        this.config.settings.dataSettings.dataRules.simple = [...this.config.settings.dataSettings.dataRules.simple];
        this.localSave();
      }
    }
  }

  removeRegexDataRule(rule: RegexDataRuleDto) {
    if (this.config !== null) {
      const index = this.config.settings.dataSettings.dataRules.regex.indexOf(rule);
      if (index !== -1) {
        this.config.settings.dataSettings.dataRules.regex.splice(index, 1);
        this.config.settings.dataSettings.dataRules.regex = [...this.config.settings.dataSettings.dataRules.regex];
        this.localSave();
      }
    }
  }

  removeLinesFromFileResource(resource: LinesFromFileResourceDto) {
    if (this.config !== null) {
      const index = this.config.settings.dataSettings.resources.linesFromFile.indexOf(resource);
      if (index !== -1) {
        this.config.settings.dataSettings.resources.linesFromFile.splice(index, 1);
        this.config.settings.dataSettings.resources.linesFromFile = [
          ...this.config.settings.dataSettings.resources.linesFromFile,
        ];
        this.localSave();
      }
    }
  }

  removeRandomLinesFromFileResource(resource: RandomLinesFromFileResourceDto) {
    if (this.config !== null) {
      const index = this.config.settings.dataSettings.resources.randomLinesFromFile.indexOf(resource);
      if (index !== -1) {
        this.config.settings.dataSettings.resources.randomLinesFromFile.splice(index, 1);
        this.config.settings.dataSettings.resources.randomLinesFromFile = [
          ...this.config.settings.dataSettings.resources.randomLinesFromFile,
        ];
        this.localSave();
      }
    }
  }

  removeCustomInput(input: CustomInputDto) {
    if (this.config !== null) {
      const index = this.config.settings.inputSettings.customInputs.indexOf(input);
      if (index !== -1) {
        this.config.settings.inputSettings.customInputs.splice(index, 1);
        this.config.settings.inputSettings.customInputs = [...this.config.settings.inputSettings.customInputs];
        this.localSave();
      }
    }
  }
}
