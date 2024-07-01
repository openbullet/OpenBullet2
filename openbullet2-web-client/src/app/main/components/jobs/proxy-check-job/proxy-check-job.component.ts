import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  faAngleLeft,
  faCheck,
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
import { JobStatus } from 'src/app/main/dtos/job/job-status';
import { PCJNewResultMessage } from 'src/app/main/dtos/job/messages/proxy-check/new-result.dto';
import { ProxyCheckJobDto } from 'src/app/main/dtos/job/proxy-check-job.dto';
import { StartConditionType } from 'src/app/main/dtos/job/start-condition.dto';
import { ProxyWorkingStatus } from 'src/app/main/enums/proxy-working-status';
import { JobService } from 'src/app/main/services/job.service';
import { ProxyCheckJobHubService } from 'src/app/main/services/proxy-check-job.hub.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { TimeSpan } from 'src/app/shared/utils/timespan';

interface LogMessage {
  timestamp: Date;
  message: string;
  color: string;
}

@Component({
  selector: 'app-proxy-check-job',
  templateUrl: './proxy-check-job.component.html',
  styleUrls: ['./proxy-check-job.component.scss'],
})
export class ProxyCheckJobComponent implements OnInit, OnDestroy {
  jobId: number | null = null;
  job: ProxyCheckJobDto | null = null;
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

  status: JobStatus = JobStatus.IDLE;
  bots = 0;
  tested = 0;
  working = 0;
  notWorking = 0;
  cpm = 0;
  elapsed = '00:00:00';
  remaining = '00:00:00';
  progress = 0;

  logsBufferSize = 10_000;
  logs: LogMessage[] = [];

  isChangingBots = false;
  desiredBots = 1;
  botLimit = 200;

  startTime: moment.Moment | null = null;
  waitLeft: TimeSpan | null = null;
  getWaitLeftTimer: ReturnType<typeof setInterval> | null = null;

  // Subscriptions
  resultSubscription: Subscription | null = null;
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
    private proxyCheckJobHubService: ProxyCheckJobHubService,
    settingsService: SettingsService,
  ) {
    activatedRoute.url.subscribe((url) => {
      this.jobId = Number.parseInt(url[2].path);
    });

    settingsService.getSystemSettings().subscribe((settings) => {
      this.botLimit = settings.botLimit;
    });
  }

  ngOnInit(): void {
    if (this.jobId === null) {
      this.router.navigate(['/jobs']);
      return;
    }

    // Mocked results, to use when debugging
    // setInterval(() => {
    //   this.onNewResult(getMockedProxyCheckJobNewResultMessage())
    // }, 50);

    this.proxyCheckJobHubService.createHubConnection(this.jobId);
    this.resultSubscription = this.proxyCheckJobHubService.result$.subscribe((result) => {
      if (result !== null) {
        this.onNewResult(result);
      }
    });

    this.tickSubscription = this.proxyCheckJobHubService.tick$.subscribe((tick) => {
      if (tick !== null) {
        this.tested = tick.tested;
        this.working = tick.working;
        this.notWorking = tick.notWorking;
        this.cpm = tick.cpm;
        this.elapsed = tick.elapsed;
        this.remaining = tick.remaining;
        this.progress = tick.progress;
      }
    });

    this.statusSubscription = this.proxyCheckJobHubService.status$.subscribe((status) => {
      if (status !== null) {
        this.onStatusChanged(status.newStatus);
      }
    });

    this.botsSubscription = this.proxyCheckJobHubService.bots$.subscribe((bots) => {
      if (bots !== null) {
        this.onBotsChanged(bots.newValue);
      }
    });

    this.taskErrorSubscription = this.proxyCheckJobHubService.taskError$.subscribe((error) => {
      if (error !== null) {
        const logMessage = `Task error for proxy ${error.proxyHost}:${error.proxyPort}: ${error.errorMessage}`;

        this.writeLog({
          timestamp: new Date(),
          message: logMessage,
          color: 'var(--fg-error)',
        });
      }
    });

    this.errorSubscription = this.proxyCheckJobHubService.error$.subscribe((error) => {
      if (error !== null) {
        this.messageService.add({
          severity: 'error',
          summary: `Error - ${error.type}`,
          detail: error.message,
        });
      }
    });

    this.completedSubscription = this.proxyCheckJobHubService.completed$.subscribe((completed) => {
      if (completed) {
        this.messageService.add({
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
    this.proxyCheckJobHubService.stopHubConnection();

    this.resultSubscription?.unsubscribe();
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
    this.jobService.getProxyCheckJob(this.jobId!).subscribe((job) => {
      this.status = job.status;
      this.bots = job.bots;
      this.tested = job.tested;
      this.working = job.working;
      this.notWorking = job.notWorking;
      this.cpm = job.cpm;
      this.elapsed = job.elapsed;
      this.remaining = job.remaining;
      this.progress = job.progress;

      if (job.startTime !== null) {
        this.startTime = moment(job.startTime);
      }

      this.job = job;
    });
  }

  onNewResult(result: PCJNewResultMessage) {
    const logMessage =
      result.workingStatus === ProxyWorkingStatus.Working
        ? `Proxy ${result.proxyHost}:${result.proxyPort} is working with ping ${result.ping} ms and country ${result.country}`
        : `Proxy ${result.proxyHost}:${result.proxyPort} is not working`;

    this.writeLog({
      timestamp: new Date(),
      message: logMessage,
      color: result.workingStatus === ProxyWorkingStatus.Working ? 'var(--fg-good)' : 'var(--fg-bad)',
    });
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
    this.router.navigate(['/job/proxy-check/edit'], { queryParams: { jobId: this.jobId } });
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
    this.jobService.skipWait(this.jobId!).subscribe();
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
}
