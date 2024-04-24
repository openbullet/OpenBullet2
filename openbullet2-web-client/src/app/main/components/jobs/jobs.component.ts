import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { faAngleLeft, faAngleRight, faBolt, faClone, faPen, faPlay, faPlus, faStop, faX } from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MessageService } from 'primeng/api';
import { JobStatus } from '../../dtos/job/job-status';
import { MultiRunJobOverviewDto } from '../../dtos/job/multi-run-job-overview.dto';
import { ProxyCheckJobOverviewDto } from '../../dtos/job/proxy-check-job-overview.dto';
import { JobService } from '../../services/job.service';
import { SettingsService } from '../../services/settings.service';
import { UserService } from '../../services/user.service';
import { GuestService } from '../../services/guest.service';

@Component({
  selector: 'app-jobs',
  templateUrl: './jobs.component.html',
  styleUrls: ['./jobs.component.scss'],
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
  faAngleLeft = faAngleLeft;
  faAngleRight = faAngleRight;

  jobsRefreshInterval: ReturnType<typeof setInterval> | null = null;
  refreshingMultiRunJobs = false;
  refreshingProxyCheckJobs = false;
  showMoreMultiRunJobs = false;
  showMoreProxyCheckJobs = false;
  createJobModalVisible = false;

  statusColor: Record<string, string> = {
    idle: 'secondary',
    waiting: 'accent',
    starting: 'good',
    running: 'good',
    pausing: 'custom',
    paused: 'custom',
    stopping: 'bad',
    resuming: 'good',
  };

  usernames: Map<number, string> = new Map();

  showJobActions = false;

  constructor(
    private jobService: JobService,
    private settingsService: SettingsService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private router: Router,
    private guestService: GuestService,
    userService: UserService
  ) {
    // If this is the admin, get the list of guests to translate
    // the Owner ID to a username
    if (userService.isAdmin()) {
      this.guestService.getAllGuests().subscribe((guests) => {
        this.usernames.set(0, userService.loadUserInfo().username);
        for (const guest of guests) {
          this.usernames.set(guest.id, guest.username);
        }
      });
    }
  }

  ngOnInit(): void {
    this.refreshJobs();

    this.settingsService.getSafeSettings().subscribe((settings) => {
      this.jobsRefreshInterval = setInterval(() => {
        this.refreshJobs();
      }, settings.generalSettings.jobManagerUpdateInterval);
    });
  }

  ngOnDestroy(): void {
    if (this.jobsRefreshInterval !== null) {
      clearInterval(this.jobsRefreshInterval);
    }
  }

  refreshJobs() {
    if (!this.refreshingMultiRunJobs) {
      this.refreshingMultiRunJobs = true;
      this.jobService.getAllMultiRunJobs().subscribe({
        next: (jobs) => {
          this.updateMultiRunJobs(jobs);
        },
        complete: () => {
          this.refreshingMultiRunJobs = false;
        },
      });
    }

    if (!this.refreshingProxyCheckJobs) {
      this.refreshingProxyCheckJobs = true;
      this.jobService.getAllProxyCheckJobs().subscribe({
        next: (jobs) => {
          this.updateProxyCheckJobs(jobs);
        },
        complete: () => {
          this.refreshingProxyCheckJobs = false;
        },
      });
    }
  }

  updateMultiRunJobs(jobs: MultiRunJobOverviewDto[]) {
    // If the job ids are different, we need to update the list
    if (this.multiRunJobs === null || jobs.length !== this.multiRunJobs.length || jobs.some((job, index) => job.id !== this.multiRunJobs![index].id)) {
      this.multiRunJobs = jobs;
      return;
    }

    // Otherwise, zip the two arrays and update the jobs
    for (const [index, updatedJob] of jobs.entries()) {
      const job = this.multiRunJobs![index];
      Object.assign(job, updatedJob);
    }
  }

  updateProxyCheckJobs(jobs: ProxyCheckJobOverviewDto[]) {
    // If the job ids are different, we need to update the list
    if (this.proxyCheckJobs === null || jobs.length !== this.proxyCheckJobs.length || jobs.some((job, index) => job.id !== this.proxyCheckJobs![index].id)) {
      this.proxyCheckJobs = jobs;
      return;
    }

    // Otherwise, zip the two arrays and update the jobs
    for (const [index, updatedJob] of jobs.entries()) {
      const job = this.proxyCheckJobs![index];
      Object.assign(job, updatedJob);
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
      accept: () => this.removeAllJobs(),
    });
  }

  abortJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    // If the status is idle, we can't abort it
    if (job.status === JobStatus.IDLE) {
      this.messageService.add({
        severity: 'error',
        summary: 'Idle',
        detail: 'The job you are trying to abort is idle, please ' + 'start it first',
      });
      return;
    }

    this.jobService.abort(job.id).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Aborted',
        detail: `Job #${job.id} was aborted`,
      });
      this.refreshJobs();
    });
  }

  toggleJobActions(event: MouseEvent) {
    event.stopPropagation();
    this.showJobActions = !this.showJobActions;
  }

  startJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    // If the status is not idle, we can't start it
    if (job.status !== JobStatus.IDLE) {
      this.messageService.add({
        severity: 'error',
        summary: 'Not idle',
        detail: 'The job you are trying to start is not idle, please ' + 'stop it or abort it first',
      });
      return;
    }

    this.jobService.start(job.id).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Started',
        detail: `Job #${job.id} was started`,
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
        detail: 'The job you are trying to edit is not idle, please ' + 'stop it or abort it first',
      });
      return;
    }

    this.router.navigate(['/job/multi-run/edit'], { queryParams: { jobId: job.id } });
  }

  editProxyCheckJob(job: ProxyCheckJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    // If the job is not idle, we can't edit it
    if (job.status !== JobStatus.IDLE) {
      this.messageService.add({
        severity: 'error',
        summary: 'Not idle',
        detail: 'The job you are trying to edit is not idle, please ' + 'stop it or abort it first',
      });
      return;
    }

    this.router.navigate(['/job/proxy-check/edit'], { queryParams: { jobId: job.id } });
  }

  cloneMultiRunJob(job: MultiRunJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    this.router.navigate(['/job/multi-run/clone'], { queryParams: { jobId: job.id } });
  }

  cloneProxyCheckJob(job: ProxyCheckJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    this.router.navigate(['/job/proxy-check/clone'], { queryParams: { jobId: job.id } });
  }

  confirmRemoveJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto, event: MouseEvent) {
    event.stopPropagation();

    if (job.status !== 'idle') {
      this.messageService.add({
        severity: 'error',
        summary: 'Not idle',
        detail: 'The job you are trying to delete is not idle, please ' + 'stop it or abort it first',
      });
      return;
    }

    this.confirmationService.confirm({
      message: `Are you sure you want to delete job #${job.id}.`,
      header: 'Are you sure?',
      icon: 'pi pi-exclamation-triangle',
      accept: () => this.removeJob(job),
    });
  }

  removeJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto) {
    this.jobService.deleteJob(job.id).subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: `Job #${job.id} was deleted`,
      });
      this.refreshJobs();
    });
  }

  removeAllJobs() {
    this.jobService.deleteAllJobs().subscribe((resp) => {
      this.messageService.add({
        severity: 'success',
        summary: 'Deleted',
        detail: 'All jobs were deleted',
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

  getUseProxyChipClass(mrj: MultiRunJobOverviewDto) {
    if (mrj.status === 'idle') {
      return 'bg-secondary';
    }

    return mrj.useProxies ? 'bg-good' : 'bg-bad';
  }
}
