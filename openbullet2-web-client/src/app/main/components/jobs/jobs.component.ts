import { Component } from '@angular/core';
import { faBolt, faPlus, faX } from '@fortawesome/free-solid-svg-icons';
import { JobService } from '../../services/job.service';
import { MultiRunJobOverviewDto } from '../../dtos/job/multi-run-job-overview.dto';
import { ProxyCheckJobOverviewDto } from '../../dtos/job/proxy-check-job-overview.dto';

@Component({
  selector: 'app-jobs',
  templateUrl: './jobs.component.html',
  styleUrls: ['./jobs.component.scss']
})
export class JobsComponent {
  multiRunJobs: MultiRunJobOverviewDto[] | null = null;
  proxyCheckJobs: ProxyCheckJobOverviewDto[] | null = null;

  faBolt = faBolt;
  faPlus = faPlus;
  faX = faX;

  constructor(private jobService: JobService) {

  }

  ngOnInit(): void {
    this.refreshJobs();
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
}
