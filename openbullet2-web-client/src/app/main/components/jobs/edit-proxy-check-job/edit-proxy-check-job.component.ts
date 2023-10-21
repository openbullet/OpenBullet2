import { Component, HostListener } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { combineLatest, map } from 'rxjs';
import { ProxyCheckJobOptionsDto } from 'src/app/main/dtos/job/proxy-check-job-options.dto';
import { JobService } from 'src/app/main/services/job.service';

enum EditMode {
  Create,
  Update,
  Clone
}

@Component({
  selector: 'app-edit-proxy-check-job',
  templateUrl: './edit-proxy-check-job.component.html',
  styleUrls: ['./edit-proxy-check-job.component.scss']
})
export class EditProxyCheckJobComponent {
  // TODO: Add a guard as well so if the user navigates away
  // from the page using the router it will also prompt the warning!
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  touched: boolean = false;
  mode: EditMode = EditMode.Update;
  jobId: number | null = null;
  options: ProxyCheckJobOptionsDto | null = null;

  constructor(
    activatedRoute: ActivatedRoute,
    private jobService: JobService
  ) {
    combineLatest([activatedRoute.url, activatedRoute.queryParams])
      .subscribe(results => {

        const uriChunks = results[0];
        const mode = uriChunks[2].path;

        if (mode === 'update') {
          this.mode = EditMode.Update;
        } else if (mode === 'clone') {
          this.mode = EditMode.Clone;
          this.touched = true;
        } else {
          this.mode = EditMode.Create;
          this.touched = true;
        }

        const queryParams = results[1];
        const jobId = queryParams['jobId'];

        if (jobId !== undefined && !isNaN(jobId)) {
          this.jobId = parseInt(jobId);
        }

        this.initJobOptions();
      });
  }

  initJobOptions() {
    // If we already have the options, we don't need to fetch them again
    if (this.options !== null) {
      return;
    }

    // If we are in update/clone mode, we need a reference job
    if (this.mode !== EditMode.Create && this.jobId === null) {
      return;
    }

    this.jobService.getProxyCheckJobOptions(this.jobId ?? -1)
      .subscribe(options => {
        this.options = options;
        console.log(options);
      });
  }
}
