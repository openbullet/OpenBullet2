import { Component, OnDestroy, OnInit } from '@angular/core';
import { faBolt, faClone, faPen, faPlay, faPlus, faStop, faX } from '@fortawesome/free-solid-svg-icons';
import { JobService } from '../../services/job.service';
import { MultiRunJobOverviewDto } from '../../dtos/job/multi-run-job-overview.dto';
import { ProxyCheckJobOverviewDto } from '../../dtos/job/proxy-check-job-overview.dto';
import { SettingsService } from '../../services/settings.service';
import { OBSettingsDto } from '../../dtos/settings/ob-settings.dto';
import { ConfirmationService, MessageService } from 'primeng/api';

@Component({
  selector: 'app-jobs',
  templateUrl: './jobs.component.html',
  styleUrls: ['./jobs.component.scss']
})
export class JobsComponent implements OnInit, OnDestroy {
  multiRunJobs: MultiRunJobOverviewDto[] | null = null;
  proxyCheckJobs: ProxyCheckJobOverviewDto[] | null = null;
  settings: OBSettingsDto | null = null;

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
    private messageService: MessageService
  ) { }

  ngOnInit(): void {
    this.refreshJobs();

    this.settingsService.getSettings()
      .subscribe(settings => {
        this.settings = settings;

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

  }

  confirmRemoveAllJobs() {

  }

  abortJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto) {

  }

  startJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto) {
    
  }

  editMultiRunJob(job: MultiRunJobOverviewDto) {

  }

  editProxyCheckJob(job: ProxyCheckJobOverviewDto) {

  }

  cloneMultiRunJob(job: MultiRunJobOverviewDto) {

  }

  cloneProxyCheckJob(job: ProxyCheckJobOverviewDto) {

  }

  confirmRemoveJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto) {
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
}
