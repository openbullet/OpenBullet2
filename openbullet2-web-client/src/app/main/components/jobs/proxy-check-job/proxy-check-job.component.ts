import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faAngleLeft, faPencil } from '@fortawesome/free-solid-svg-icons';
import { JobStatus } from 'src/app/main/dtos/job/job-status';
import { PCJNewResultMessage } from 'src/app/main/dtos/job/messages/proxy-check/new-result.dto';
import { ProxyCheckJobDto } from 'src/app/main/dtos/job/proxy-check-job.dto';
import { getMockedProxyCheckJobNewResultMessage } from 'src/app/main/mock/messages.mock';
import { JobService } from 'src/app/main/services/job.service';
import { ProxyCheckJobHubService } from 'src/app/main/services/proxy-check-job.hub.service';
import { TimeSpan } from 'src/app/shared/utils/timespan';

@Component({
  selector: 'app-proxy-check-job',
  templateUrl: './proxy-check-job.component.html',
  styleUrls: ['./proxy-check-job.component.scss']
})
export class ProxyCheckJobComponent implements OnInit, OnDestroy {
  jobId: number | null = null;
  job: ProxyCheckJobDto | null = null;
  faAngleLeft = faAngleLeft;
  faPencil = faPencil;

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
  tested: number = 0;
  working: number = 0;
  notWorking: number = 0;
  cpm: number = 0;
  elapsed: string = '00:00:00';
  remaining: string = '00:00:00';
  progress: number = 0;

  constructor(
    activatedRoute: ActivatedRoute,
    private router: Router,
    private jobService: JobService,
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
        this.status = status.newStatus;
      }
    });

    this.jobService.getProxyCheckJob(this.jobId)
      .subscribe(job => {
        this.job = job;
        this.tested = job.tested;
        this.working = job.working;
        this.notWorking = job.notWorking;
        this.cpm = job.cpm;
        this.elapsed = job.elapsed;
        this.remaining = job.remaining;
        this.progress = job.progress;
      });
  }

  ngOnDestroy(): void {
    this.proxyCheckJobHubService.stopHubConnection();
  }

  onNewResult(result: PCJNewResultMessage) {
    console.log(result);
  }

  canEdit() {
    return this.job?.status === JobStatus.IDLE;
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
}
