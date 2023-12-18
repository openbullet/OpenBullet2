import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faAngleLeft, faCheck, faForward, faPause, faPen, faPlay, faStop, faX } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { MessageService } from 'primeng/api';
import { Subscription } from 'rxjs';
import { JobStatus } from 'src/app/main/dtos/job/job-status';
import { ChangeBotsMessage } from 'src/app/main/dtos/job/messages/change-bots.dto';
import { MRJNewHitMessage } from 'src/app/main/dtos/job/messages/multi-run/hit.dto';
import { MRJNewResultMessage } from 'src/app/main/dtos/job/messages/multi-run/new-result.dto';
import { JobProxyMode } from 'src/app/main/dtos/job/multi-run-job-options.dto';
import { MRJDataStatsDto, MRJHitDto, MultiRunJobDto } from 'src/app/main/dtos/job/multi-run-job.dto';
import { StartConditionType } from 'src/app/main/dtos/job/start-condition.dto';
import { getMockedMultiRunJobNewResultMessage } from 'src/app/main/mock/messages.mock';
import { JobService } from 'src/app/main/services/job.service';
import { MultiRunJobHubService } from 'src/app/main/services/multi-run-job.hub.service';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { TimeSpan } from 'src/app/shared/utils/timespan';

interface LogMessage {
  timestamp: Date;
  message: string;
  color: string;
}

@Component({
  selector: 'app-multi-run-job',
  templateUrl: './multi-run-job.component.html',
  styleUrls: ['./multi-run-job.component.scss']
})
export class MultiRunJobComponent {
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
  Math = Math;
  JobStatus = JobStatus;
  StartConditionType = StartConditionType;
  JobProxyMode = JobProxyMode;

  statusColor: Record<JobStatus, string> = {
    idle: 'secondary',
    waiting: 'accent',
    starting: 'good',
    running: 'good',
    pausing: 'custom',
    paused: 'custom',
    stopping: 'bad',
    resuming: 'good'
  };

  status: JobStatus = JobStatus.IDLE;
  bots: number = 0;

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
    tested: 0
  };
  
  proxyStats = {
    total: 0,
    alive: 0,
    bad: 0,
    banned: 0
  };

  cpm: number = 0;
  captchaCredit: number = 0;
  elapsed: string = '00:00:00';
  remaining: string = '00:00:00';
  progress: number = 0;

  hits: MRJHitDto[] = [];

  logsBufferSize: number = 10_000;
  logs: LogMessage[] = [];

  isChangingBots: boolean = false;
  desiredBots: number = 1;

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

  constructor(
    activatedRoute: ActivatedRoute,
    private router: Router,
    private jobService: JobService,
    private messageService: MessageService,
    private multiRunJobHubService: MultiRunJobHubService,
  ) {
    activatedRoute.url.subscribe(url => {
      this.jobId = parseInt(url[2].path);
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
    this.resultSubscription = this.multiRunJobHubService.result$
    .subscribe(result => {
      if (result !== null) {
        this.onNewResult(result);
      }
    });

    this.hitSubscription = this.multiRunJobHubService.hit$
    .subscribe(hit => {
      if (hit !== null) {
        this.onNewHit(hit);
      }
    });

    this.tickSubscription = this.multiRunJobHubService.tick$
    .subscribe(tick => {
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

    this.statusSubscription = this.multiRunJobHubService.status$
    .subscribe(status => {
      if (status !== null) {
        this.onStatusChanged(status.newStatus);
      }
    });

    this.botsSubscription = this.multiRunJobHubService.bots$
    .subscribe(bots => {
      if (bots !== null) {
        this.onBotsChanged(bots.newValue);
      }
    });

    this.taskErrorSubscription = this.multiRunJobHubService.taskError$
    .subscribe(error => {
      if (error !== null) {
        let logMessage = `Task error (${error.dataLine})`;

        if (error.proxy !== null) {
          logMessage += ` (${error.proxy.host}:${error.proxy.port})`;
        }

        logMessage += `: ${error.errorMessage}`;

        this.writeLog({
          timestamp: new Date(),
          message: logMessage,
          color: 'var(--fg-error)'
        });
      }
    });

    this.errorSubscription = this.multiRunJobHubService.error$
    .subscribe(error => {
      if (error !== null) {
        this.messageService.add({
          severity: 'error',
          summary: `Error - ${error.type}`,
          detail: error.message
        });
      }
    });

    this.completedSubscription = this.multiRunJobHubService.completed$
    .subscribe(completed => {
      if (completed) {
        this.messageService.add({
          severity: 'success',
          summary: 'Completed',
          detail: 'Job completed'
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
  }

  getJobData() {
    this.jobService.getMultiRunJob(this.jobId!)
      .subscribe(job => {
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

        this.job = job;
      });
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
      color: color
    });
  }

  onNewHit(hitMessage: MRJNewHitMessage) {
    this.hits.push(hitMessage.hit);
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
      color: 'var(--fg-primary)'
    });
  }

  onBotsChanged(bots: number) {
    this.bots = bots;

    const logMessage = `Bots changed to ${bots}`;

    this.writeLog({
      timestamp: new Date(),
      message: logMessage,
      color: 'var(--fg-primary)'
    });
  }

  canEdit() {
    return this.status === JobStatus.IDLE;
  }

  editSettings() {
    this.router.navigate(
      [`/job/multi-run/edit`], 
      { queryParams: { jobId: this.jobId } }
    );
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
    console.log('START');
    this.jobService.start(this.jobId!).subscribe();
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
    return this.status === JobStatus.RUNNING ||
      this.status === JobStatus.PAUSED ||
      this.status === JobStatus.PAUSING ||
      this.status === JobStatus.STOPPING;
  }

  abort() {
    this.jobService.abort(this.jobId!).subscribe();
  }

  canSkipWait() {
    return this.status === JobStatus.WAITING;
  }

  skipWait() {
    this.jobService.start(this.jobId!, true).subscribe();
  }

  showEditBotsInput() {
    this.desiredBots = this.bots;
    this.isChangingBots = true;
  }

  changeBots(bots: number) {
    this.jobService.changeBots(this.jobId!, bots).subscribe();

    const logMessage = `Requested to change bots to ${bots}`;

    this.writeLog({
      timestamp: new Date(),
      message: logMessage,
      color: 'var(--fg-primary)'
    });

    // If we decrease the bots while the job is running, it
    // might take some time
    const slow = this.bots > bots && this.status === JobStatus.RUNNING;

    this.messageService.add({
      severity: 'info',
      summary: 'Requested',
      detail: `Requested to change bots to ${bots}`
        + (slow ? '. This might take some time' : '')
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

    return TimeSpan.fromTime(
      duration.days(),
      duration.hours(),
      duration.minutes(),
      duration.seconds(),
      0
    );
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
    return this.Math.min(
      this.dataStats.tested + this.job!.skip,
      this.dataStats.total
    );
  }
}
