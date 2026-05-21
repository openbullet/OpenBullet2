import { Component, HostListener, OnInit } from '@angular/core';
import { faPlus, faTriangleExclamation, faWrench, faX } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import { AutoCompleteCompleteEvent } from 'primeng/autocomplete';
import {
  BrowserAutomationEngine,
  ConfigDto,
  CustomInputDto,
  LinesFromFileResourceDto,
  RandomLinesFromFileResourceDto,
  RegexDataRuleDto,
  SimpleDataRuleDto,
  StringRule,
} from 'src/app/main/dtos/config/config.dto';
import { TestDataRulesResultDto } from 'src/app/main/dtos/config/test-data-rules.dto';
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
  browserAutomationEngines: BrowserAutomationEngine[] = [BrowserAutomationEngine.Puppeteer];
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
  testDataForRules = '';
  testWordlistTypeForRules = '';
  ruleTestResult: TestDataRulesResultDto | null = null;
  dataRuleSliceSuggestions: string[] = [];

  constructor(
    private configService: ConfigService,
    private settingsService: SettingsService,
    private messageService: MessageService,
  ) {
    this.configService.selectedConfig$.subscribe((config) => {
      this.config = config;
      this.clearRuleTestResults();
      this.ensureRuleTestWordlistType();
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
      this.ensureRuleTestWordlistType();
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
      this.clearRuleTestResults();
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
      this.clearRuleTestResults();
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
        this.clearRuleTestResults();
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
        this.clearRuleTestResults();
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

  testDataRules() {
    if (this.config === null) {
      return;
    }

    const validationErrors = this.getDataRuleValidationErrors();
    if (validationErrors.length > 0) {
      this.clearRuleTestResults();
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid Data Rules',
        detail: validationErrors.length === 1
          ? validationErrors[0]
          : `${validationErrors.length} data rule fields need attention. Fix the highlighted rules and try again.`,
      });
      return;
    }

    this.ruleTestResult = null;

    this.configService.testDataRules({
      testData: this.testDataForRules,
      wordlistType: this.testWordlistTypeForRules,
      dataRules: this.config.settings.dataSettings.dataRules,
    }).subscribe({
      next: (result) => {
        this.ruleTestResult = result;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Rule Test Error',
          detail: error?.error?.message ?? 'Could not test the data rules',
        });
      },
    });
  }

  onDataRuleChanged() {
    this.clearRuleTestResults();
  }

  clearRuleTestResults() {
    this.ruleTestResult = null;
  }

  filterDataRuleSliceSuggestions(event: AutoCompleteCompleteEvent) {
    const trimmedQuery = event.query.trim().toLowerCase();
    const allSuggestions = this.getDataRuleSliceSuggestions();

    this.dataRuleSliceSuggestions = trimmedQuery === ''
      ? allSuggestions
      : allSuggestions.filter(s => s.toLowerCase().includes(trimmedQuery));
  }

  private ensureRuleTestWordlistType() {
    if (this.wordlistTypes.length === 0) {
      return;
    }

    if (this.wordlistTypes.includes(this.testWordlistTypeForRules)) {
      return;
    }

    const allowedWordlistType = this.config?.settings.dataSettings.allowedWordlistTypes
      .find((type) => this.wordlistTypes.includes(type));

    this.testWordlistTypeForRules = allowedWordlistType ?? this.wordlistTypes[0];
  }

  hasInvalidDataRules(): boolean {
    return this.getDataRuleValidationErrors().length > 0;
  }

  isSimpleDataRuleSliceNameInvalid(rule: SimpleDataRuleDto): boolean {
    return this.isBlank(rule.sliceName);
  }

  getSimpleDataRuleSliceNameError(rule: SimpleDataRuleDto): string | null {
    return this.isSimpleDataRuleSliceNameInvalid(rule)
      ? 'Slice name cannot be empty.'
      : null;
  }

  isSimpleDataRuleStringToCompareInvalid(rule: SimpleDataRuleDto): boolean {
    if (this.isBlank(rule.stringToCompare)) {
      return true;
    }

    return this.requiresNumericComparisonValue(rule.comparison)
      && !this.isInteger(rule.stringToCompare);
  }

  getSimpleDataRuleStringToCompareError(rule: SimpleDataRuleDto): string | null {
    if (this.isBlank(rule.stringToCompare)) {
      return 'Compared value cannot be empty.';
    }

    if (this.requiresNumericComparisonValue(rule.comparison) && !this.isInteger(rule.stringToCompare)) {
      return 'Compared value must be a whole number for this comparison.';
    }

    return null;
  }

  isRegexDataRuleSliceNameInvalid(rule: RegexDataRuleDto): boolean {
    return this.isBlank(rule.sliceName);
  }

  getRegexDataRuleSliceNameError(rule: RegexDataRuleDto): string | null {
    return this.isRegexDataRuleSliceNameInvalid(rule)
      ? 'Slice name cannot be empty.'
      : null;
  }

  isRegexDataRulePatternInvalid(rule: RegexDataRuleDto): boolean {
    return this.isBlank(rule.regexToMatch);
  }

  getRegexDataRulePatternError(rule: RegexDataRuleDto): string | null {
    return this.isRegexDataRulePatternInvalid(rule)
      ? 'Regular expression cannot be empty.'
      : null;
  }

  private getDataRuleSliceSuggestions(): string[] {
    if (this.envSettings === null) {
      return [];
    }

    const allowedWordlistTypes = this.config?.settings.dataSettings.allowedWordlistTypes ?? [];
    const wordlistTypes = allowedWordlistTypes.length > 0
      ? this.envSettings.wordlistTypes.filter(w => allowedWordlistTypes.includes(w.name))
      : this.envSettings.wordlistTypes;

    return [...new Set(wordlistTypes
      .flatMap(w => w.slices.concat(w.slicesAlias))
      .map(s => s.trim())
      .filter(s => s.length > 0))]
      .sort((a, b) => a.localeCompare(b));
  }

  private getDataRuleValidationErrors(): string[] {
    if (this.config === null) {
      return [];
    }

    const errors: string[] = [];

    this.config.settings.dataSettings.dataRules.simple.forEach((rule, index) => {
      const sliceNameError = this.getSimpleDataRuleSliceNameError(rule);
      if (sliceNameError !== null) {
        errors.push(`Simple rule #${index + 1}: ${sliceNameError}`);
      }

      const stringToCompareError = this.getSimpleDataRuleStringToCompareError(rule);
      if (stringToCompareError !== null) {
        errors.push(`Simple rule #${index + 1}: ${stringToCompareError}`);
      }
    });

    this.config.settings.dataSettings.dataRules.regex.forEach((rule, index) => {
      const sliceNameError = this.getRegexDataRuleSliceNameError(rule);
      if (sliceNameError !== null) {
        errors.push(`Regex rule #${index + 1}: ${sliceNameError}`);
      }

      const patternError = this.getRegexDataRulePatternError(rule);
      if (patternError !== null) {
        errors.push(`Regex rule #${index + 1}: ${patternError}`);
      }
    });

    return errors;
  }

  private requiresNumericComparisonValue(comparison: StringRule): boolean {
    return comparison === StringRule.LongerThan || comparison === StringRule.ShorterThan;
  }

  private isBlank(value: string | null | undefined): boolean {
    return value === null || value === undefined || value.trim().length === 0;
  }

  private isInteger(value: string): boolean {
    return /^-?\d+$/.test(value.trim());
  }
}
