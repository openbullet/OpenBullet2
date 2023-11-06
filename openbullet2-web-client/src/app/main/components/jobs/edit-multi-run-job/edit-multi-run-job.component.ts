import { Component, HostListener, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faBolt, faGears, faPlus, faSave, faX } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Observable, combineLatest } from 'rxjs';
import { ConfigInfoDto } from 'src/app/main/dtos/config/config-info.dto';
import { CustomWebhookHitOutput, DataPoolType, DiscordWebhookHitOutput, HitOutputType, HitOutputTypes, JobProxyMode, MultiRunJobOptionsDto, NoValidProxyBehaviour, ProxySourceType, ProxySourceTypes, TelegramBotHitOutput } from 'src/app/main/dtos/job/multi-run-job-options.dto';
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
  
  startConditionMode: StartConditionMode = StartConditionMode.Absolute;
  startAfter: TimeSpan = new TimeSpan(0);
  startAt: Date = moment().add(1, 'days').toDate();

  selectedConfigInfo: ConfigInfoDto | null = null;

  defaultProxyGroup = {
    id: -1,
    name: 'All',
    owner: { id: -2, username: 'System' }
  };

  selectedProxyGroup: ProxyGroupDto = this.defaultProxyGroup;

  fieldsValidity: { [key: string] : boolean; } = {};
  touched: boolean = false;

  selectConfigModalVisible: boolean = false;
  configureDiscordWebhookHitOutputModalVisible: boolean = false;
  configureTelegramBotHitOutputModalVisible: boolean = false;
  configureCustomWebhookHitOutputModalVisible: boolean = false;

  constructor(
    activatedRoute: ActivatedRoute,
    private proxyGroupService: ProxyGroupService,
    private settingsService: SettingsService,
    private jobService: JobService,
    private configService: ConfigService,
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

        this.configService.getInfo(options.configId).subscribe(configInfo => {
          this.selectedConfigInfo = configInfo;
        });

        this.options = options;
      });
  }

  onValidityChange(validity: FieldValidity) {
    this.fieldsValidity = {
      ...this.fieldsValidity,
      [validity.key]: validity.valid
    };
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

  // Can accept if touched and every field is valid
  canAccept() {
    return this.touched && Object.values(this.fieldsValidity).every(v => v);
  }

  accept() {
    if (this.options === null) {
      return;
    }

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

  selectConfig(config: ConfigInfoDto) {
    this.selectedConfigInfo = config;
    this.options!.configId = config.id;
    this.selectConfigModalVisible = false;
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
}
