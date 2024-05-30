import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  faAlignLeft,
  faAngleLeft,
  faBug,
  faCheck,
  faCircleDot,
  faCopy,
  faForward,
  faPause,
  faPen,
  faPlay,
  faStop,
  faX,
} from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { MessageService } from 'primeng/api';
import { Subscription } from 'rxjs';
import { ConfigMode } from 'src/app/main/dtos/config/config-info.dto';
import { JobStatus } from 'src/app/main/dtos/job/job-status';
import { MRJNewHitMessage } from 'src/app/main/dtos/job/messages/multi-run/hit.dto';
import { MRJNewResultMessage } from 'src/app/main/dtos/job/messages/multi-run/new-result.dto';
import { JobProxyMode } from 'src/app/main/dtos/job/multi-run-job-options.dto';
import { MRJDataStatsDto, MRJHitDto, MultiRunJobDto } from 'src/app/main/dtos/job/multi-run-job.dto';
import { StartConditionType } from 'src/app/main/dtos/job/start-condition.dto';
import { getMockedMultiRunJobNewResultMessage } from 'src/app/main/mock/messages.mock';
import { ConfigDebuggerSettingsService } from 'src/app/main/services/config-debugger-settings.service';
import { ConfigService } from 'src/app/main/services/config.service';
import { JobService } from 'src/app/main/services/job.service';
import { MultiRunJobHubService } from 'src/app/main/services/multi-run-job.hub.service';
import { UserService } from 'src/app/main/services/user.service';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { TimeSpan } from 'src/app/shared/utils/timespan';
import { HitLogComponent } from './hit-log/hit-log.component';
import { ConfigSection, JobDisplayMode, SafeOBSettingsDto } from 'src/app/main/dtos/settings/ob-settings.dto';
import { SettingsService } from 'src/app/main/services/settings.service';
import { CustomInputAnswerDto, CustomInputQuestionDto } from 'src/app/main/dtos/job/custom-inputs.dto';
import { BotDetailsDto } from 'src/app/main/dtos/job/multi-run-job-bot-details.dto';

interface LogMessage {
  timestamp: Date;
  message: string;
  color: string;
}

enum HitType {
  Success = 'SUCCESS',
  Custom = 'CUSTOM',
  ToCheck = 'NONE',
}

@Component({
  selector: 'app-multi-run-job',
  templateUrl: './multi-run-job.component.html',
  styleUrls: ['./multi-run-job.component.scss'],
})
export class MultiRunJobComponent implements OnInit, OnDestroy {
  jobId: number | null = null;
  job: MultiRunJobDto | null = null;
  faAngleLeft = faAngleLeft;
  faPen = faPen;
  faPause = faPause;
  faPlay = faPlay;
  faStop = faStop;
  faForward = faForward;
  faX = faX;
  faCheck = faCheck;
  faCopy = faCopy;
  faCircleDot = faCircleDot;
  faAlignLeft = faAlignLeft;
  faBug = faBug;
  Math = Math;
  JobStatus = JobStatus;
  StartConditionType = StartConditionType;
  JobProxyMode = JobProxyMode;
  HitType = HitType;

  settings: SafeOBSettingsDto | null = null;
  customInputs: CustomInputQuestionDto[] | null = null;

  statusColor: Record<JobStatus, string> = {
    idle: 'secondary',
    waiting: 'accent',
    starting: 'good',
    running: 'good',
    pausing: 'custom',
    paused: 'custom',
    stopping: 'bad',
    resuming: 'good',
  };

  customStatuses: string[] = ['CUSTOM'];
  botLimit = 200;

  status: JobStatus = JobStatus.IDLE;
  bots = 0;

  dataStats: MRJDataStatsDto = {
    hits: 0,
    custom: 0,
    fails: 0,
    invalid: 0,
    retried: 0,
    banned: 0,
    errors: 0,
    toCheck: 0,
    total: 0,
    tested: 0,
  };

  proxyStats = {
    total: 0,
    alive: 0,
    bad: 0,
    banned: 0,
  };

  cpm = 0;
  captchaCredit = 0;
  elapsed = '00:00:00';
  remaining = '00:00:00';
  progress = 0;
  userRole = 'guest';

  hitLogModalVisible = false;
  customInputsModalVisible = false;

  @ViewChild('hitLogComponent')
  hitLogComponent: HitLogComponent | undefined = undefined;

  @ViewChild('hitSoundPlayer') hitSoundPlayer!: ElementRef;

  hits: MRJHitDto[] = [];
  filteredHits: MRJHitDto[] = [];
  selectedHits: MRJHitDto[] = [];
  lastSelectedHit: MRJHitDto | null = null;
  hitTypes: HitType[] = [HitType.Success, HitType.Custom, HitType.ToCheck];
  selectedHitType: HitType = HitType.Success;

  logsBufferSize = 200;
  logs: LogMessage[] = [];

  isChangingBots = false;
  desiredBots = 1;

  startTime: moment.Moment | null = null;
  waitLeft: TimeSpan | null = null;
  getWaitLeftTimer: ReturnType<typeof setInterval> | null = null;

  // Subscriptions
  resultSubscription: Subscription | null = null;
  hitSubscription: Subscription | null = null;
  tickSubscription: Subscription | null = null;
  statusSubscription: Subscription | null = null;
  botsSubscription: Subscription | null = null;
  taskErrorSubscription: Subscription | null = null;
  errorSubscription: Subscription | null = null;
  completedSubscription: Subscription | null = null;

  botsRefreshInterval: ReturnType<typeof setInterval> | null = null;
  botDetails: BotDetailsDto[] = [];

  constructor(
    activatedRoute: ActivatedRoute,
    private router: Router,
    private jobService: JobService,
    private messageService: MessageService,
    private multiRunJobHubService: MultiRunJobHubService,
    private debuggerSettingsService: ConfigDebuggerSettingsService,
    private configService: ConfigService,
    settingsService: SettingsService,
    userService: UserService,
  ) {
    // We are under the assumption that this Observable will immediately
    // emit the current value, otherwise the component will not work
    activatedRoute.url.subscribe((url) => {
      this.jobId = Number.parseInt(url[2].path);
    });

    this.userRole = userService.loadUserInfo().role.toLocaleLowerCase();

    settingsService.getSafeSettings().subscribe((settings) => {
      this.settings = settings;

      if (settings.generalSettings.defaultJobDisplayMode === JobDisplayMode.Detailed) {
        this.botsRefreshInterval = setInterval(() => {
          this.refreshBotDetails();
        }, 1000);
      }
    });

    settingsService.getEnvironmentSettings().subscribe((envSettings) => {
      this.customStatuses = envSettings.customStatuses.map((s) => s.name);
    });

    settingsService.getSystemSettings().subscribe((settings) => {
      this.botLimit = settings.botLimit;
    });

    this.jobService.getCustomInputs(this.jobId!).subscribe((customInputs) => {
      this.customInputs = customInputs;
    });
  }

  ngOnInit(): void {
    if (this.jobId === null) {
      this.router.navigate(['/jobs']);
      return;
    }

    // Mocked results, to use when debugging
    // setInterval(() => {
    //   this.onNewResult(getMockedMultiRunJobNewResultMessage())
    // }, 50);

    this.multiRunJobHubService.createHubConnection(this.jobId);
    this.resultSubscription = this.multiRunJobHubService.result$.subscribe((result) => {
      if (result !== null) {
        this.onNewResult(result);
      }
    });

    this.hitSubscription = this.multiRunJobHubService.hit$.subscribe((hit) => {
      if (hit !== null) {
        this.onNewHit(hit);
      }
    });

    this.tickSubscription = this.multiRunJobHubService.tick$.subscribe((tick) => {
      if (tick !== null) {
        this.dataStats = tick.dataStats;
        this.proxyStats = tick.proxyStats;
        this.cpm = tick.cpm;
        this.captchaCredit = tick.captchaCredit;
        this.elapsed = tick.elapsed;
        this.remaining = tick.remaining;
        this.progress = tick.progress;
      }
    });

    this.statusSubscription = this.multiRunJobHubService.status$.subscribe((status) => {
      if (status !== null) {
        this.onStatusChanged(status.newStatus);
      }
    });

    this.botsSubscription = this.multiRunJobHubService.bots$.subscribe((bots) => {
      if (bots !== null) {
        this.onBotsChanged(bots.newValue);
      }
    });

    this.taskErrorSubscription = this.multiRunJobHubService.taskError$.subscribe((error) => {
      if (error !== null) {
        let logMessage = `Task error (${error.dataLine})`;

        if (error.proxy !== null) {
          logMessage += ` (${error.proxy.host}:${error.proxy.port})`;
        }

        logMessage += `: ${error.errorMessage}`;

        this.writeLog({
          timestamp: new Date(),
          message: logMessage,
          color: 'var(--fg-error)',
        });
      }
    });

    this.errorSubscription = this.multiRunJobHubService.error$.subscribe((error) => {
      if (error !== null) {
        this.messageService.add({
          key: 'tc',
          severity: 'error',
          summary: `Error - ${error.type}`,
          detail: error.message
        });
      }
    });

    this.completedSubscription = this.multiRunJobHubService.completed$.subscribe((completed) => {
      if (completed) {
        this.messageService.add({
          key: 'tc',
          severity: 'success',
          summary: 'Completed',
          detail: 'Job completed',
        });

        this.getJobData();
      }
    });

    this.getJobData();

    this.getWaitLeftTimer = setInterval(() => {
      this.waitLeft = this.getWaitLeft();
    }, 1000);
  }

  ngOnDestroy(): void {
    this.multiRunJobHubService.stopHubConnection();

    this.resultSubscription?.unsubscribe();
    this.hitSubscription?.unsubscribe();
    this.tickSubscription?.unsubscribe();
    this.statusSubscription?.unsubscribe();
    this.botsSubscription?.unsubscribe();
    this.taskErrorSubscription?.unsubscribe();
    this.errorSubscription?.unsubscribe();
    this.completedSubscription?.unsubscribe();

    if (this.getWaitLeftTimer !== null) {
      clearInterval(this.getWaitLeftTimer);
    }

    if (this.botsRefreshInterval !== null) {
      clearInterval(this.botsRefreshInterval);
    }
  }

  getJobData() {
    this.jobService.getMultiRunJob(this.jobId!).subscribe((job) => {
      this.status = job.status;
      this.bots = job.bots;
      this.dataStats = job.dataStats;
      this.proxyStats = job.proxyStats;
      this.cpm = job.cpm;
      this.captchaCredit = job.captchaCredit;
      this.elapsed = job.elapsed;
      this.remaining = job.remaining;
      this.progress = job.progress;

      if (job.startTime !== null) {
        this.startTime = moment(job.startTime);
      }

      this.selectedHits = [];
      this.lastSelectedHit = null;
      this.hits = job.hits;
      this.chooseHitType(this.selectedHitType);

      this.job = job;
    });
  }

  refreshBotDetails() {
    if (this.status === JobStatus.RUNNING ||
      this.status === JobStatus.PAUSING ||
      this.status === JobStatus.RESUMING ||
      this.status === JobStatus.STOPPING
    ) {
      this.jobService.getBotDetails(this.jobId!).subscribe((botDetails) => {
        this.botDetails = botDetails;
      });
    }
  }

  onNewResult(result: MRJNewResultMessage) {
    let color = 'var(--fg-custom)';

    switch (result.status) {
      case 'SUCCESS':
        color = 'var(--fg-good)';
        break;

      case 'FAIL':
        color = 'var(--fg-bad)';
        break;

      case 'BAN':
        color = 'var(--fg-banned)';
        break;

      case 'RETRY':
        color = 'var(--fg-retry)';
        break;

      case 'NONE':
        color = 'var(--fg-tocheck)';
        break;

      case 'ERROR':
        color = 'var(--fg-error)';
        break;
    }

    let logMessage = `Line checked (${result.dataLine})`;

    if (result.proxy !== null) {
      logMessage += ` (${result.proxy.host}:${result.proxy.port})`;
    }

    logMessage += ` - ${result.status}`;

    this.writeLog({
      timestamp: new Date(),
      message: logMessage,
      color: color,
    });
  }

  onNewHit(hitMessage: MRJNewHitMessage) {
    this.hits.push(hitMessage.hit);

    if (hitMessage.hit.type === this.selectedHitType) {
      this.filteredHits.push(hitMessage.hit);
    } else if (this.customStatuses.includes(hitMessage.hit.type) && this.selectedHitType === HitType.Custom) {
      this.filteredHits.push(hitMessage.hit);
    }

    if (this.settings?.customizationSettings.playSoundOnHit && hitMessage.hit.type === 'SUCCESS') {
      // Note: if multiple hits are received at the same time, the sound will only play once
      // to avoid spamming the sound
      this.hitSoundPlayer.nativeElement.play();
    }
  }

  onStatusChanged(status: JobStatus) {
    this.status = status;

    if (status === JobStatus.WAITING) {
      this.startTime = moment();
    }

    const logMessage = `Status changed to ${status}`;

    this.writeLog({
      timestamp: new Date(),
      message: logMessage,
      color: 'var(--fg-primary)',
    });
  }

  onBotsChanged(bots: number) {
    this.bots = bots;

    const logMessage = `Bots changed to ${bots}`;

    this.writeLog({
      timestamp: new Date(),
      message: logMessage,
      color: 'var(--fg-primary)',
    });
  }

  canEdit() {
    return this.status === JobStatus.IDLE;
  }

  editSettings() {
    this.router.navigate(['/job/multi-run/edit'], { queryParams: { jobId: this.jobId } });
  }

  backToJobs() {
    this.router.navigate(['/jobs']);
  }

  canPause() {
    return this.status === JobStatus.RUNNING;
  }

  pause() {
    this.jobService.pause(this.jobId!).subscribe();
  }

  canStart() {
    return this.status === JobStatus.IDLE;
  }

  start() {
    // If custom inputs weren't set, show the modal
    if (
      this.customInputs !== null &&
      this.customInputs.length > 0 &&
      this.customInputs.some(i => i.currentAnswer === null)
    ) {
      this.messageService.add({
        key: 'tc',
        severity: 'warn',
        summary: 'Missing inputs',
        detail: 'Please set custom inputs before starting the job',
      });
      this.showCustomInputs();
      return;
    }

    this.jobService.start(this.jobId!).subscribe();

    // Clear the hits
    this.hits = [];
    this.filteredHits = [];
    this.selectedHits = [];
    this.lastSelectedHit = null;
  }

  canStop() {
    return this.status === JobStatus.RUNNING || this.status === JobStatus.PAUSED;
  }

  stop() {
    this.jobService.stop(this.jobId!).subscribe();
  }

  canResume() {
    return this.status === JobStatus.PAUSED;
  }

  resume() {
    this.jobService.resume(this.jobId!).subscribe();
  }

  canAbort() {
    return (
      this.status === JobStatus.STARTING ||
      this.status === JobStatus.RUNNING ||
      this.status === JobStatus.PAUSED ||
      this.status === JobStatus.PAUSING ||
      this.status === JobStatus.STOPPING
    );
  }

  abort() {
    this.jobService.abort(this.jobId!).subscribe();
  }

  canSkipWait() {
    return this.status === JobStatus.WAITING;
  }

  skipWait() {
    this.jobService.skipWait(this.jobId!, true).subscribe();
  }

  showEditBotsInput() {
    this.desiredBots = this.bots;
    this.isChangingBots = true;
  }

  changeBots(bots: number) {
    if (bots < 1 || bots > this.botLimit) {
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid bots',
        detail: `Bots must be between 1 and ${this.botLimit}`,
      });

      return;
    }

    this.jobService.changeBots(this.jobId!, bots).subscribe();

    const logMessage = `Requested to change bots to ${bots}`;

    this.writeLog({
      timestamp: new Date(),
      message: logMessage,
      color: 'var(--fg-primary)',
    });

    // If we decrease the bots while the job is running, it
    // might take some time
    const slow = this.bots > bots && this.status === JobStatus.RUNNING;

    this.messageService.add({
      key: 'tc',
      severity: 'info',
      summary: 'Requested',
      detail: `Requested to change bots to ${bots}${slow ? '. This might take some time' : ''}`,
    });

    this.isChangingBots = false;
    this.bots = bots;
  }

  writeLog(message: LogMessage) {
    // If there are more than N logs, shift the array
    // to always keep up to N logs
    if (this.logs.length > this.logsBufferSize) {
      this.logs.pop();
    }

    this.logs.unshift(message);
  }

  getWaitLeft(): TimeSpan | null {
    if (this.job === null || this.startTime === null) {
      return null;
    }

    let startAt = moment();

    if (this.job.startCondition._polyTypeName === StartConditionType.Absolute) {
      // If the wait is absolute, we already know when it will start
      startAt = moment(this.job.startCondition.startAt);
    } else if (this.job.startCondition._polyTypeName === StartConditionType.Relative) {
      // If the wait is relative, we need to add the startAfter to the startTime
      const startAfter = parseTimeSpan(this.job.startCondition.startAfter);
      startAt = moment(this.startTime)
        .add(startAfter.days, 'days')
        .add(startAfter.hours, 'hours')
        .add(startAfter.minutes, 'minutes')
        .add(startAfter.seconds, 'seconds');
    }

    const diff = moment(startAt).diff(moment());
    const duration = moment.duration(diff);

    return TimeSpan.fromTime(duration.days(), duration.hours(), duration.minutes(), duration.seconds(), 0);
  }

  shouldUseProxies(): boolean {
    if (this.job === null) {
      return false;
    }

    switch (this.job.proxyMode) {
      case JobProxyMode.Off:
        return false;
      case JobProxyMode.On:
        return true;
      case JobProxyMode.Default:
        return this.job.config?.needsProxies ?? false;
    }
  }

  getTestedCount() {
    return this.Math.min(this.dataStats.tested + this.job!.skip, this.dataStats.total);
  }

  chooseHitType(type: HitType) {
    this.selectedHitType = type;

    switch (type) {
      case HitType.Success:
        this.filteredHits = this.hits.filter((h) => h.type === 'SUCCESS');
        break;

      case HitType.Custom:
        this.filteredHits = this.hits.filter((h) => this.customStatuses.includes(h.type));
        break;

      case HitType.ToCheck:
        this.filteredHits = this.hits.filter((h) => h.type === 'NONE');
        break;
    }

    this.selectedHits = [];
    this.lastSelectedHit = null;
  }

  isHitSelected(hit: MRJHitDto): boolean {
    return this.selectedHits.includes(hit);
  }

  onHitClicked(hit: MRJHitDto, event: MouseEvent) {
    // If the user is holding shift, select all hits between the last
    // selected hit and the current one
    if (event.shiftKey && this.lastSelectedHit !== null) {
      const lastSelectedHitIndex = this.filteredHits.indexOf(this.lastSelectedHit);
      const currentHitIndex = this.filteredHits.indexOf(hit);

      if (lastSelectedHitIndex === -1 || currentHitIndex === -1) {
        return;
      }

      const min = this.Math.min(lastSelectedHitIndex, currentHitIndex);
      const max = this.Math.max(lastSelectedHitIndex, currentHitIndex);

      this.selectedHits = this.filteredHits.slice(min, max + 1);
      this.lastSelectedHit = hit;
      return;
    }

    // If the user is holding ctrl, toggle the hit selection
    if (event.ctrlKey) {
      if (this.isHitSelected(hit)) {
        this.selectedHits = this.selectedHits.filter((h) => h !== hit);
      } else {
        this.selectedHits.push(hit);
      }

      this.lastSelectedHit = hit;
      return;
    }

    // Otherwise, just select the hit
    this.selectedHits = [hit];
    this.lastSelectedHit = hit;
  }

  copyHitData(withCapture: boolean) {
    // If no hit selected, show error
    if (this.selectedHits.length === 0) {
      this.messageService.add({
        key: 'tc',
        severity: 'error',
        summary: 'Invalid',
        detail: 'Please select at least one hit to copy data',
      });
      return;
    }

    // Copy the data of the selected hits to the clipboard
    const data = this.selectedHits
      .map((h) => {
        let result = h.data;

        if (withCapture) {
          result += ` | ${h.capturedData}`;
        }

        return result;
      })
      .join('\n');

    navigator.clipboard.writeText(data);
    this.messageService.add({
      key: 'tc',
      severity: 'success',
      summary: 'Copied',
      detail: 'Copied hits data to clipboard',
    });
  }

  sendToDebugger() {
    // If no hit selected or more than 1 hit selected, show error
    if (this.selectedHits.length !== 1) {
      this.messageService.add({
        key: 'tc',
        severity: 'error',
        summary: 'Invalid',
        detail: 'Please select one hit to send to the debugger',
      });
      return;
    }

    const hit = this.selectedHits[0];

    const debuggerSettings = this.debuggerSettingsService.loadLocalSettings();
    debuggerSettings.testData = hit.data;

    if (hit.proxy !== null) {
      debuggerSettings.useProxy = true;
      debuggerSettings.proxyType = hit.proxy.type;
      debuggerSettings.testProxy = hit.proxy.username === null || hit.proxy.username.length === 0
        ? `(${hit.proxy.type})${hit.proxy.host}:${hit.proxy.port}`
        : `(${hit.proxy.type})${hit.proxy.host}:${hit.proxy.port}:${hit.proxy.username}:${hit.proxy.password}`;
    }

    this.debuggerSettingsService.saveLocalSettings(debuggerSettings);

    if (this.settings === null) {
      this.messageService.add({
        key: 'tc',
        severity: 'error',
        summary: 'Error',
        detail: 'Settings not loaded yet',
      });
      return;
    }

    // If there is a selected config, redirect to the correct page
    // basing on the config's mode
    const config = this.configService.selectedConfig;

    if (config === null) {
      this.messageService.add({
        key: 'tc',
        severity: 'error',
        summary: 'Error',
        detail: 'No config selected',
      });
      return;
    }

    const configSection = this.settings.generalSettings.configSectionOnLoad;
    let route = '';

    switch (config.mode) {
      case ConfigMode.Stack:
      case ConfigMode.LoliCode:
        switch (configSection) {
          case ConfigSection.Stacker:
            route = 'config/stacker';
            break;

          case ConfigSection.LoliCode:
            route = 'config/lolicode';
            break;

          case ConfigSection.CSharpCode:
            route = 'config/csharp';
            break;
        }
        break;

      case ConfigMode.CSharp:
        route = 'config/csharp';
        break;

      case ConfigMode.Legacy:
        route = 'config/loliscript';
        break;
    }

    if (route === '') {
      return;
    }

    this.router.navigate([route]);
  }

  showFullLog() {
    // If no hit selected or more than 1 hit selected, show error
    if (this.selectedHits.length !== 1) {
      this.messageService.add({
        key: 'tc',
        severity: 'error',
        summary: 'Invalid',
        detail: 'Please select one hit to show the full log',
      });
      return;
    }

    if (this.hitLogComponent === undefined) {
      return;
    }

    this.hitLogComponent.getHitLog(this.selectedHits[0].id);
    this.hitLogModalVisible = true;
  }

  hitTypeDisplayFunction(hitType: HitType): string {
    return hitType.toString();
  }

  showCustomInputs() {
    this.customInputsModalVisible = true;
  }

  setCustomInputs(answers: CustomInputAnswerDto[]) {
    this.jobService.setCustomInputs({
      jobId: this.jobId!,
      answers: answers
    }).subscribe(() => {
      this.messageService.add({
        key: 'tc',
        severity: 'success',
        summary: 'Success',
        detail: 'Custom inputs set successfully'
      });
      this.customInputsModalVisible = false;
      this.jobService.getCustomInputs(this.jobId!).subscribe((customInputs) => {
        this.customInputs = customInputs;
      });
    });
  }

  getHitColorClass(hitType: string | HitType) {
    if (hitType === 'SUCCESS') {
      return 'color-good';
    }

    if (hitType === 'NONE') {
      return 'color-tocheck';
    }

    return 'color-custom';
  }
}
