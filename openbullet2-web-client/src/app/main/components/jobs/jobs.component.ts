import { Component, OnDestroy, OnInit } from '@angular/core';
import { faBolt, faClone, faPen, faPlay, faPlus, faStop, faX } from '@fortawesome/free-solid-svg-icons';
import { JobService } from '../../services/job.service';
import { MultiRunJobOverviewDto } from '../../dtos/job/multi-run-job-overview.dto';
import { ProxyCheckJobOverviewDto } from '../../dtos/job/proxy-check-job-overview.dto';

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

  constructor(private jobService: JobService) {

  }

  ngOnInit(): void {
    this.refreshJobs();

    this.intervalId = setInterval(() => {
      this.refreshJobs();
    }, 1000); // TODO: This number should be read from settings
  }

  ngOnDestroy(): void {
    clearInterval(this.intervalId);
  }

  refreshJobs() {
    this.jobService.getAllMultiRunJobs()
      .subscribe(jobs => this.multiRunJobs = jobs);

    this.jobService.getAllProxyCheckJobs()
      .subscribe(jobs => this.proxyCheckJobs = jobs);
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

  removeJob(job: MultiRunJobOverviewDto | ProxyCheckJobOverviewDto) {

  }
}
