import { Component, OnDestroy, OnInit } from '@angular/core';
import { faBolt, faClone, faPen, faPlay, faPlus, faStop, faX } from '@fortawesome/free-solid-svg-icons';
import { JobService } from '../../services/job.service';
import { MultiRunJobOverviewDto } from '../../dtos/job/multi-run-job-overview.dto';
import { ProxyCheckJobOverviewDto } from '../../dtos/job/proxy-check-job-overview.dto';
import { SettingsService } from '../../services/settings.service';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Router } from '@angular/router';
import { JobStatus } from '../../dtos/job/job-status';

@Component({
  selector: 'app-jobs',
  templateUrl: './jobs.component.html',
  styleUrls: ['./jobs.component.scss']
})
export class JobsComponent implements OnInit, OnDestroy {
  multiRunJobs: MultiRunJobOverviewDto[] | null = null;
  proxyCheckJobs: ProxyCheckJobOverviewDto[] | null = null;

  faBolt = faBolt;
  faPlus = faPlus;
  faX = faX;
  faStop = faStop;
  faPlay = faPlay;
  faPen = faPen;
  faClone = faClone;

  intervalId: any;
  refreshingMultiRunJobs: boolean = false;
  refreshingProxyCheckJobs: boolean = false;
  showMoreMultiRunJobs: boolean = false;
  showMoreProxyCheckJobs: boolean = false;
  createJobModalVisible = false;

  statusColor: Record<string, string> = {
    idle: 'secondary',
    waiting: 'accent',
    starting: 'good',
    running: 'good',
    pausing: 'custom',
    paused: 'custom',
    stopping: 'bad',
    resuming: 'good'
  };

  proxyColor: Record<string, string> = {
    on: 'good',
    off: 'bad',
    default: 'secondary'
  };

  constructor(
    private jobService: JobService,
    private settingsService: SettingsService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.refreshJobs();

    this.settingsService.getSafeSettings()
      .subscribe(settings => {
        this.intervalId = setInterval(() => {
          this.refreshJobs();
        }, settings.generalSettings.jobManagerUpdateInterval);
      });
  }

  ngOnDestroy(): void {
    clearInterval(this.intervalId);
  }

  refreshJobs() {
    if (!this.refreshingMultiRunJobs) {
      this.refreshingMultiRunJobs = true;
      this.jobService.getAllMultiRunJobs()
        .subscribe({
          next: jobs => {
            this.multiRunJobs = jobs;
            this.refreshingMultiRunJobs = false;
          },
          error: () => {
            this.refreshingMultiRunJobs = false;
          }
        });
    }

    if (!this.refreshingProxyCheckJobs) {
      this.refreshingProxyCheckJobs = true;
      this.jobService.getAllProxyCheckJobs()
        .subscribe({
          next: jobs => {
            this.proxyCheckJobs = jobs;
            this.refreshingProxyCheckJobs = false;
          },
          error: () => {
            this.refreshingProxyCheckJobs = false;
          }
        });
    }
  }

  openCreateJobModal() {
    this.createJobModalVisible = true;
  }

  confirmRemoveAllJobs() {
    this.confirmationService.confirm({
      message: 'Are you sure you want to delete all jobs?',
      header: 'Are you sure?',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.removeAllJobs()
    });
  }

  abortJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto,
    event: MouseEvent) {
    event.stopPropagation();

    // If the status is idle, we can't abort it
    if (job.status === JobStatus.IDLE) {
      this.messageService.add({
        severity: 'error',
        summary: 'Idle',
        detail: 'The job you are trying to abort is idle, please' +
          'start it first'
      });
      return;
    }

    this.jobService.abort(job.id)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Aborted',
          detail: `Job #${job.id} was aborted`
        });
        this.refreshJobs();
      });
  }

  startJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto,
    event: MouseEvent) {
    event.stopPropagation();

    // If the status is not idle, we can't start it
    if (job.status !== JobStatus.IDLE) {
      this.messageService.add({
        severity: 'error',
        summary: 'Not idle',
        detail: 'The job you are trying to start is not idle, please' +
          'stop it or abort it first'
      });
      return;
    }

    this.jobService.start(job.id)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Started',
          detail: `Job #${job.id} was started`
        });
        this.refreshJobs();
      });
  }

  editMultiRunJob(job: MultiRunJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    // If the job is not idle, we can't edit it
    if (job.status !== JobStatus.IDLE) {
      this.messageService.add({
        severity: 'error',
        summary: 'Not idle',
        detail: 'The job you are trying to edit is not idle, please' +
          'stop it or abort it first'
      });
      return;
    }

    this.router.navigate(
      [`/job/multi-run/edit`],
      { queryParams: { jobId: job.id } }
    );
  }

  editProxyCheckJob(job: ProxyCheckJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    // If the job is not idle, we can't edit it
    if (job.status !== JobStatus.IDLE) {
      this.messageService.add({
        severity: 'error',
        summary: 'Not idle',
        detail: 'The job you are trying to edit is not idle, please' +
          'stop it or abort it first'
      });
      return;
    }

    this.router.navigate(
      [`/job/proxy-check/edit`],
      { queryParams: { jobId: job.id } }
    );
  }

  cloneMultiRunJob(job: MultiRunJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    this.router.navigate(
      [`/job/multi-run/clone`],
      { queryParams: { jobId: job.id } }
    );
  }

  cloneProxyCheckJob(job: ProxyCheckJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    this.router.navigate(
      [`/job/proxy-check/clone`],
      { queryParams: { jobId: job.id } }
    );
  }

  confirmRemoveJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto,
    event: MouseEvent) {
    event.stopPropagation();

    if (job.status !== 'idle') {
      this.messageService.add({
        severity: 'error',
        summary: 'Not idle',
        detail: 'The job you are trying to delete is not idle, please' +
          'stop it or abort it first'
      });
      return;
    }

    this.confirmationService.confirm({
      message: `Are you sure you want to delete job #${job.id}.`,
      header: 'Are you sure?',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.removeJob(job)
    });
  }

  removeJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto) {
    this.jobService.deleteJob(job.id)
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: `Job #${job.id} was deleted`
        });
        this.refreshJobs();
      });
  }

  removeAllJobs() {
    this.jobService.deleteAllJobs()
      .subscribe(resp => {
        this.messageService.add({
          severity: 'success',
          summary: 'Deleted',
          detail: 'All jobs were deleted'
        });
        this.refreshJobs();
      });
  }

  viewProxyCheckJob(pcj: ProxyCheckJobOverviewDto) {
    this.router.navigate([`/job/proxy-check/${pcj.id}`]);
  }

  viewMultiRunJob(mrj: MultiRunJobOverviewDto) {
    this.router.navigate([`/job/multi-run/${mrj.id}`]);
  }
}
