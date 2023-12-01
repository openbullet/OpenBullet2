import { Component, HostListener, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faBolt, faGears, faPlus, faSave, faX } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Observable, combineLatest } from 'rxjs';
import { ConfigInfoDto } from 'src/app/main/dtos/config/config-info.dto';
import { CombinationsDataPool, CustomWebhookHitOutput, DataPoolType, DiscordWebhookHitOutput, HitOutputType, HitOutputTypes, JobProxyMode, MultiRunJobOptionsDto, NoValidProxyBehaviour, ProxySourceType, ProxySourceTypes, TelegramBotHitOutput, WordlistDataPool } from 'src/app/main/dtos/job/multi-run-job-options.dto';
import { StartConditionMode } from 'src/app/main/dtos/job/start-condition-mode';
import { StartConditionType } from 'src/app/main/dtos/job/start-condition.dto';
import { ProxyGroupDto } from 'src/app/main/dtos/proxy-group/proxy-group.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { JobService } from 'src/app/main/services/job.service';
import { ProxyGroupService } from 'src/app/main/services/proxy-group.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { DeactivatableComponent } from 'src/app/shared/guards/can-deactivate-form.guard';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { TimeSpan } from 'src/app/shared/utils/timespan';
import { SelectConfigComponent } from '../select-config/select-config.component';
import { ProxyType } from 'src/app/main/enums/proxy-type';
import { ConfigureDiscordComponent } from './configure-discord/configure-discord.component';
import { ConfigureTelegramComponent } from './configure-telegram/configure-telegram.component';
import { ConfigureCustomWebhookComponent } from './configure-custom-webhook/configure-custom-webhook.component';
import { WordlistType } from 'src/app/main/dtos/settings/environment-settings.dto';
import { SelectWordlistComponent } from '../select-wordlist/select-wordlist.component';
import { WordlistDto } from 'src/app/main/dtos/wordlist/wordlist.dto';
import { WordlistService } from 'src/app/main/services/wordlist.service';

enum EditMode {
  Create = 'create',
  Edit = 'edit',
  Clone = 'clone'
}

@Component({
  selector: 'app-edit-multi-run-job',
  templateUrl: './edit-multi-run-job.component.html',
  styleUrls: ['./edit-multi-run-job.component.scss']
})
export class EditMultiRunJobComponent implements DeactivatableComponent {
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  @ViewChild('selectConfigComponent') 
  selectConfigComponent: SelectConfigComponent | undefined;

  @ViewChild('selectWordlistComponent')
  selectWordlistComponent: SelectWordlistComponent | undefined;

  @ViewChild('configureDiscordComponent')
  configureDiscordComponent: ConfigureDiscordComponent | undefined;

  @ViewChild('configureTelegramComponent')
  configureTelegramComponent: ConfigureTelegramComponent | undefined;

  @ViewChild('configureCustomWebhookComponent')
  configureCustomWebhookComponent: ConfigureCustomWebhookComponent | undefined;

  faBolt = faBolt;
  faGears = faGears;
  faPlus = faPlus;
  faX = faX;
  faSave = faSave;

  Object = Object;
  Math = Math;
  StartConditionMode = StartConditionMode;
  JobProxyMode = JobProxyMode;
  jobProxyModes = [
    JobProxyMode.Default,
    JobProxyMode.On,
    JobProxyMode.Off
  ];
  noValidProxyBehaviours = [
    NoValidProxyBehaviour.DoNothing,
    NoValidProxyBehaviour.Unban,
    NoValidProxyBehaviour.Reload
  ];
  DataPoolType = DataPoolType;
  ProxySourceType = ProxySourceType;
  HitOutputType = HitOutputType;
  proxyTypes = [
    ProxyType.Http,
    ProxyType.Socks4,
    ProxyType.Socks5,
    ProxyType.Socks4a
  ];

  mode: EditMode = EditMode.Edit;
  jobId: number | null = null;
  options: MultiRunJobOptionsDto | null = null;
  proxyGroups: ProxyGroupDto[] | null = null;
  wordlistTypes: string[] | null = null;
  
  startConditionMode: StartConditionMode = StartConditionMode.Absolute;
  startAfter: TimeSpan = new TimeSpan(0);
  startAt: Date = moment().add(1, 'days').toDate();

  dataPoolType: DataPoolType = DataPoolType.Wordlist;

  // We save info about data pools here so when the user switches between them
  // using the radio buttons we don't lose the data
  dataPoolWordlistType: string = 'Default';
  dataPoolWordlistId: number = -1; // -1 if not present
  dataPoolFileName: string = '';
  dataPoolRangeStart: number = 0;
  dataPoolRangeAmount: number = 0;
  dataPoolRangeStep: number = 1;
  dataPoolRangePad: boolean = false;
  dataPoolCombinationsCharSet: string = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
  dataPoolCombinationsLength: number = 4;

  selectedConfigInfo: ConfigInfoDto | null = null;
  selectedWordlist: WordlistDto | null = null;

  defaultProxyGroup = {
    id: -1,
    name: 'All',
    owner: { id: -2, username: 'System' }
  };

  selectedProxyGroup: ProxyGroupDto = this.defaultProxyGroup;

  fieldsValidity: { [key: string] : boolean; } = {};
  touched: boolean = false;

  selectConfigModalVisible: boolean = false;
  selectWordlistModalVisible: boolean = false;
  configureDiscordWebhookHitOutputModalVisible: boolean = false;
  configureTelegramBotHitOutputModalVisible: boolean = false;
  configureCustomWebhookHitOutputModalVisible: boolean = false;

  constructor(
    activatedRoute: ActivatedRoute,
    private proxyGroupService: ProxyGroupService,
    private settingsService: SettingsService,
    private jobService: JobService,
    private configService: ConfigService,
    private wordlistService: WordlistService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) {
    combineLatest([activatedRoute.url, activatedRoute.queryParams])
      .subscribe(results => {

        const uriChunks = results[0];

        this.mode = <EditMode>uriChunks[2].path;

        const queryParams = results[1];
        const jobId = queryParams['jobId'];

        if (jobId !== undefined && !isNaN(jobId)) {
          this.jobId = parseInt(jobId);
        }

        this.initJobOptions();
      });

    this.proxyGroupService.getAllProxyGroups()
      .subscribe(proxyGroups => {
        this.proxyGroups = [
          this.defaultProxyGroup,
          ...proxyGroups
        ];
      });

    this.settingsService.getEnvironmentSettings()
      .subscribe(settings => {
        this.wordlistTypes = settings.wordlistTypes.map(wt => wt.name);
        this.dataPoolWordlistType = settings.wordlistTypes[0].name;
      });
  }

  canDeactivate() {
    if (!this.touched) {
      return true;
    }

    // Ask for confirmation and return the observable
    return new Observable<boolean>(observer => {
      this.confirmationService.confirm({
        message: `You have unsaved changes. Are you sure that you want to leave?`,
        header: 'Confirmation',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          observer.next(true);
          observer.complete();
        },
        reject: () => {
          observer.next(false);
          observer.complete();
        }
      });
    });
  }

  initJobOptions() {
    // If we already have the options, we don't need to fetch them again
    if (this.options !== null) {
      return;
    }

    // If we are in update/clone mode, we need a reference job
    if (this.mode !== EditMode.Create && this.jobId === null) {
      return;
    }

    this.jobService.getMultiRunJobOptions(this.jobId ?? -1)
      .subscribe(options => {
        if (options.startCondition._polyTypeName === StartConditionType.Relative) {
          this.startAfter = parseTimeSpan(options.startCondition.startAfter);
          this.startConditionMode = StartConditionMode.Relative;
        } else if (options.startCondition._polyTypeName === StartConditionType.Absolute) {
          this.startAt = moment(options.startCondition.startAt).toDate();
          this.startConditionMode = StartConditionMode.Absolute;
        }

        if (options.dataPool._polyTypeName === DataPoolType.Wordlist) {
          this.dataPoolType = DataPoolType.Wordlist;
          this.dataPoolWordlistId = options.dataPool.wordlistId;
        } else if (options.dataPool._polyTypeName === DataPoolType.File) {
          this.dataPoolType = DataPoolType.File;
          this.dataPoolWordlistType = options.dataPool.wordlistType;
          this.dataPoolFileName = options.dataPool.fileName;
        } else if (options.dataPool._polyTypeName === DataPoolType.Range) {
          this.dataPoolType = DataPoolType.Range;
          this.dataPoolWordlistType = options.dataPool.wordlistType;
          this.dataPoolRangeStart = options.dataPool.start;
          this.dataPoolRangeAmount = options.dataPool.amount;
          this.dataPoolRangeStep = options.dataPool.step;
          this.dataPoolRangePad = options.dataPool.pad;
        } else if (options.dataPool._polyTypeName === DataPoolType.Combinations) {
          this.dataPoolType = DataPoolType.Combinations;
          this.dataPoolWordlistType = options.dataPool.wordlistType;
          this.dataPoolCombinationsCharSet = options.dataPool.charSet;
          this.dataPoolCombinationsLength = options.dataPool.length;
        } else if (options.dataPool._polyTypeName === DataPoolType.Infinite) {
          this.dataPoolType = DataPoolType.Infinite;
        }

        this.configService.getInfo(options.configId).subscribe(configInfo => {
          this.selectedConfigInfo = configInfo;
        });
        
        // If the data pool is a wordlist, we need to fetch it
        if (this.dataPoolType === DataPoolType.Wordlist) {
          this.wordlistService.getWordlist(this.dataPoolWordlistId).subscribe(wordlist => {
            this.selectedWordlist = wordlist;
          });
        }

        this.options = options;
      });
  }

  onValidityChange(validity: FieldValidity) {
    this.fieldsValidity = {
      ...this.fieldsValidity,
      [validity.key]: validity.valid
    };
  }

  onDataPoolTypeChange(type: DataPoolType) {
    this.dataPoolType = type;
    this.touched = true;
  }

  onStartConditionModeChange(mode: StartConditionMode) {
    this.startConditionMode = mode;
    this.touched = true;
  }

  onStartAfterChange(timeSpan: TimeSpan) {
    this.startAfter = timeSpan;
    
    if (this.startConditionMode === StartConditionMode.Relative) {
      this.options!.startCondition = {
        _polyTypeName: StartConditionType.Relative,
        startAfter: this.startAfter.toString()
      };
    }
  }

  onStartAtChange(date: Date) {
    // Convert to local time, otherwise it's UTC
    this.startAt = moment(date).local().toDate();
    this.touched = true;

    if (this.startConditionMode === StartConditionMode.Absolute) {
      this.options!.startCondition = {
        _polyTypeName: StartConditionType.Absolute,
        startAt: this.startAt.toISOString()
      };
    }
  }

  configureDataPool() {
    if (this.options === null) {
      return;
    }

    switch (this.dataPoolType) {
      case DataPoolType.Wordlist:
        this.options.dataPool = {
          _polyTypeName: DataPoolType.Wordlist,
          wordlistId: this.dataPoolWordlistId
        };
        break;
      case DataPoolType.File:
        this.options.dataPool = {
          _polyTypeName: DataPoolType.File,
          wordlistType: this.dataPoolWordlistType,
          fileName: this.dataPoolFileName
        };
        break;
      case DataPoolType.Range:
        this.options.dataPool = {
          _polyTypeName: DataPoolType.Range,
          wordlistType: this.dataPoolWordlistType,
          start: this.dataPoolRangeStart,
          amount: this.dataPoolRangeAmount,
          step: this.dataPoolRangeStep,
          pad: this.dataPoolRangePad
        };
        break;
      case DataPoolType.Combinations:
        this.options.dataPool = {
          _polyTypeName: DataPoolType.Combinations,
          wordlistType: this.dataPoolWordlistType,
          charSet: this.dataPoolCombinationsCharSet,
          length: this.dataPoolCombinationsLength
        };
        break;
      case DataPoolType.Infinite:
        this.options.dataPool = {
          _polyTypeName: DataPoolType.Infinite
        };
        break;
    }
  }

  // Can accept if touched and every field is valid
  canAccept() {
    return this.touched && Object.values(this.fieldsValidity).every(v => v);
  }

  accept() {
    if (this.options === null) {
      return;
    }

    this.configureDataPool();

    if (this.mode === EditMode.Create) {
      this.jobService.createMultiRunJob(this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Created',
            detail: `Multi run job ${resp.id} was created`
          });
          this.router.navigate([`/job/multi-run/${resp.id}`]);
        });
    } else if (this.mode === EditMode.Edit) {
      this.jobService.updateMultiRunJob(this.jobId!, this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Updated',
            detail: `Multi run job ${resp.id} was updated`
          });
          this.router.navigate([`/job/multi-run/${resp.id}`]);
        });
    } else if (this.mode === EditMode.Clone) {
      this.jobService.createMultiRunJob(this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Cloned',
            detail: `Multi run job ${resp.id} was cloned from ${this.jobId}`
          });
          this.router.navigate([`/job/multi-run/${resp.id}`]);
        });
    }
  }

  openSelectConfigModal() {
    this.selectConfigModalVisible = true;
    this.selectConfigComponent?.refresh();
  }

  openSelectWordlistModal() {
    this.selectWordlistModalVisible = true;
    this.selectWordlistComponent?.refresh();
  }

  selectConfig(config: ConfigInfoDto) {
    this.selectedConfigInfo = config;
    this.options!.configId = config.id;
    this.selectConfigModalVisible = false;
    this.touched = true;
  }

  selectWordlist(wordlist: WordlistDto) {
    this.selectedWordlist = wordlist;
    this.dataPoolWordlistId = wordlist.id;
    this.selectWordlistModalVisible = false;
    this.touched = true;
  }

  jobProxyModeDisplayFunction(mode: JobProxyMode) {
    switch (mode) {
      case JobProxyMode.Default:
        return this.selectedConfigInfo === null || this.selectedConfigInfo === undefined
          ? 'Default'
          : `Default (${this.selectedConfigInfo.needsProxies})`;
      case JobProxyMode.On:
        return 'On';
      case JobProxyMode.Off:
        return 'Off';
    }
  }

  noValidProxyBehaviourDisplayFunction(mode: NoValidProxyBehaviour) {
    switch (mode) {
      case NoValidProxyBehaviour.DoNothing:
        return 'Do nothing';
      case NoValidProxyBehaviour.Unban:
        return 'Unban the existing proxies';
      case NoValidProxyBehaviour.Reload:
        return 'Reload the proxies from the sources';
    }
  }

  addGroupProxySource() {
    this.options!.proxySources.push({
      _polyTypeName: ProxySourceType.Group,
      groupId: -1
    });
    this.touched = true;
  }

  addFileProxySource() {
    this.options!.proxySources.push({
      _polyTypeName: ProxySourceType.File,
      fileName: '',
      defaultType: ProxyType.Http
    });
    this.touched = true;
  }

  addRemoteProxySource() {
    this.options!.proxySources.push({
      _polyTypeName: ProxySourceType.Remote,
      url: '',
      defaultType: ProxyType.Http
    });
    this.touched = true;
  }

  removeProxySource(proxySource: ProxySourceTypes) {
    this.options!.proxySources = this.options!.proxySources
      .filter(ps => ps !== proxySource);
    this.touched = true;
  }

  addDatabaseHitOutput() {
    // If there is already a database hit output, don't add another one
    if (this.options!.hitOutputs.some(
      ho => ho._polyTypeName === HitOutputType.Database)) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Already exists',
        detail: 'You can only have a single database hit output'
      });
      return;
    }

    this.options!.hitOutputs.push({
      _polyTypeName: HitOutputType.Database
    });
    this.touched = true;
  }

  addFileSystemHitOutput() {
    this.options!.hitOutputs.push({
      _polyTypeName: HitOutputType.FileSystem,
      baseDir: ''
    });
    this.touched = true;
  }

  addDiscordWebhookHitOutput() {
    this.options!.hitOutputs.push({
      _polyTypeName: HitOutputType.DiscordWebhook,
      webhook: 'https://discord.com/api/webhooks/...',
      username: '',
      avatarUrl: '',
      onlyHits: true
    });
    this.touched = true;
  }

  addTelegramBotHitOutput() {
    this.options!.hitOutputs.push({
      _polyTypeName: HitOutputType.TelegramBot,
      apiServer: 'https://api.telegram.org/',
      token: '',
      chatId: 0,
      onlyHits: true
    });
    this.touched = true;
  }

  addCustomWebhookHitOutput() {
    this.options!.hitOutputs.push({
      _polyTypeName: HitOutputType.CustomWebhook,
      url: 'http://mycustomwebhook.com',
      user: 'Anonymous',
      onlyHits: true
    });
    this.touched = true;
  }

  removeHitOutput(hitOutput: HitOutputTypes) {
    this.options!.hitOutputs = this.options!.hitOutputs
      .filter(ho => ho !== hitOutput);
    this.touched = true;
  }

  openEditDiscordWebhookHitOutputModal(hitOutput: DiscordWebhookHitOutput) {
    this.configureDiscordComponent!.setHitOutput(hitOutput);
    this.configureDiscordWebhookHitOutputModalVisible = true;
  }

  updateDiscordWebhookHitOutput() {
    this.touched = true;
    this.configureDiscordWebhookHitOutputModalVisible = false;
  }

  openEditTelegramBotHitOutputModal(hitOutput: TelegramBotHitOutput) {
    this.configureTelegramComponent!.setHitOutput(hitOutput);
    this.configureTelegramBotHitOutputModalVisible = true;
  }

  updateTelegramBotHitOutput() {
    this.touched = true;
    this.configureTelegramBotHitOutputModalVisible = false;
  }

  openEditCustomWebhookHitOutputModal(hitOutput: CustomWebhookHitOutput) {
    this.configureCustomWebhookComponent!.setHitOutput(hitOutput);
    this.configureCustomWebhookHitOutputModalVisible = true;
  }

  updateCustomWebhookHitOutput() {
    this.touched = true;
    this.configureCustomWebhookHitOutputModalVisible = false;
  }

  calcCombinations() {
    return Math.pow(
      this.dataPoolCombinationsCharSet.length,
      this.dataPoolCombinationsLength
    );
  }

  calcCombinationsTime(cpm: number): TimeSpan {
    const combinations = this.calcCombinations();
    const seconds = combinations / cpm * 60;
    return new TimeSpan(seconds * 1000);
  }

  calcRange(): string {
    // Output (e.g. start = 1, step = 2):
    // amount = 0: Nothing
    // amount = 1: start
    // amount > 1: [start, start + step, start + 2 * step, ...] (up to 5)

    if (this.dataPoolRangeAmount === 0) {
      return 'Nothing';
    } else if (this.dataPoolRangeAmount === 1) {
      return this.dataPoolRangeStart.toString();
    }

    const range = [];
    for (let i = 0; i < Math.min(this.dataPoolRangeAmount - 1, 5); i++) {
      range.push(this.dataPoolRangeStart + i * this.dataPoolRangeStep);
    }

    const useEllipsis = this.dataPoolRangeAmount > 6;

    const lastNumber = this.dataPoolRangeStart + 
      (this.dataPoolRangeAmount - 1) * this.dataPoolRangeStep;

    const lastNumberDigits = lastNumber.toString().length;

    if (this.dataPoolRangePad) {
      const padLength = Math.max(
        this.dataPoolRangeStart.toString().length,
        lastNumberDigits
      );

      return range
        .map(n => n.toString().padStart(padLength, '0'))
        .join(', ') + (useEllipsis ? ', ... ' : ', ') + lastNumber.toString().padStart(padLength, '0');
    }

    return range.join(', ') + (useEllipsis ? ', ... ' : ', ') + lastNumber;
  }
}
