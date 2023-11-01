import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faAngleLeft, faCheck, faForward, faPause, faPen, faPlay, faStop, faX } from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import { JobStatus } from 'src/app/main/dtos/job/job-status';
import { ChangeBotsMessage } from 'src/app/main/dtos/job/messages/change-bots.dto';
import { PCJNewResultMessage } from 'src/app/main/dtos/job/messages/proxy-check/new-result.dto';
import { ProxyCheckJobDto } from 'src/app/main/dtos/job/proxy-check-job.dto';
import { ProxyWorkingStatus } from 'src/app/main/enums/proxy-working-status';
import { getMockedProxyCheckJobNewResultMessage } from 'src/app/main/mock/messages.mock';
import { JobService } from 'src/app/main/services/job.service';
import { ProxyCheckJobHubService } from 'src/app/main/services/proxy-check-job.hub.service';

interface LogMessage {
  timestamp: Date;
  message: string;
  color: string;
}

@Component({
  selector: 'app-proxy-check-job',
  templateUrl: './proxy-check-job.component.html',
  styleUrls: ['./proxy-check-job.component.scss']
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
  tested: number = 0;
  working: number = 0;
  notWorking: number = 0;
  cpm: number = 0;
  elapsed: string = '00:00:00';
  remaining: string = '00:00:00';
  progress: number = 0;

  logs: LogMessage[] = [];

  isChangingBots: boolean = false;
  desiredBots: number = 1;

  constructor(
    activatedRoute: ActivatedRoute,
    private router: Router,
    private jobService: JobService,
    private messageService: MessageService,
    private proxyCheckJobHubService: ProxyCheckJobHubService
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
    /*
    setInterval(() => {
      this.onNewResult(getMockedProxyCheckJobNewResultMessage())
    }, 1000);
    */

    this.proxyCheckJobHubService.createHubConnection(this.jobId);
    this.proxyCheckJobHubService.result$.subscribe(result => {
      if (result !== null) {
        this.onNewResult(result);
      }
    });

    this.proxyCheckJobHubService.tick$.subscribe(tick => {
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

    this.proxyCheckJobHubService.status$.subscribe(status => {
      if (status !== null) {
        this.onStatusChanged(status.newStatus);
      }
    });

    this.proxyCheckJobHubService.bots$.subscribe(bots => {
      if (bots !== null) {
        this.onBotsChanged(bots.newValue);
      }
    });

    this.jobService.getProxyCheckJob(this.jobId)
      .subscribe(job => {
        this.status = job.status;
        this.bots = job.bots;
        this.tested = job.tested;
        this.working = job.working;
        this.notWorking = job.notWorking;
        this.cpm = job.cpm;
        this.elapsed = job.elapsed;
        this.remaining = job.remaining;
        this.progress = job.progress;

        this.job = job;
      });
  }

  ngOnDestroy(): void {
    this.proxyCheckJobHubService.stopHubConnection();
  }

  onNewResult(result: PCJNewResultMessage) {
    const logMessage = result.workingStatus === ProxyWorkingStatus.Working
      ? `Proxy ${result.proxyHost}:${result.proxyPort} is working with ping ${result.ping} ms and country ${result.country}`
      : `Proxy ${result.proxyHost}:${result.proxyPort} is not working`;

    this.logs.unshift({
      timestamp: new Date(),
      message: logMessage,
      color: result.workingStatus === ProxyWorkingStatus.Working ? 'var(--fg-good)' : 'var(--fg-bad)'
    });
  }

  onStatusChanged(status: JobStatus) {
    this.status = status;

    const logMessage = `Status changed to ${status}`;

    this.logs.unshift({
      timestamp: new Date(),
      message: logMessage,
      color: 'var(--fg-primary)'
    });
  }

  onBotsChanged(bots: number) {
    this.bots = bots;

    const logMessage = `Bots changed to ${bots}`;

    this.logs.unshift({
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
      [`/job/proxy-check/edit`], 
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
    this.proxyCheckJobHubService.pause();
  }

  canStart() {
    return this.status === JobStatus.IDLE;
  }

  start() {
    this.proxyCheckJobHubService.start();
  }

  canStop() {
    return this.status === JobStatus.RUNNING || this.status === JobStatus.PAUSED;
  }

  stop() {
    this.proxyCheckJobHubService.stop();
  }

  canResume() {
    return this.status === JobStatus.PAUSED;
  }

  resume() {
    this.proxyCheckJobHubService.resume();
  }

  canAbort() {
    return this.status === JobStatus.RUNNING ||
      this.status === JobStatus.PAUSED ||
      this.status === JobStatus.PAUSING ||
      this.status === JobStatus.STOPPING;
  }

  abort() {
    this.proxyCheckJobHubService.abort();
  }

  canSkipWait() {
    return this.status === JobStatus.WAITING;
  }

  skipWait() {
    this.proxyCheckJobHubService.skipWait();
  }

  showEditBotsInput() {
    this.desiredBots = this.bots;
    this.isChangingBots = true;
  }

  changeBots(bots: number) {
    this.proxyCheckJobHubService.changeBots(
      <ChangeBotsMessage>{ desired: bots }
    );

    const logMessage = `Requested to change bots to ${bots}`;

    this.logs.unshift({
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
}
