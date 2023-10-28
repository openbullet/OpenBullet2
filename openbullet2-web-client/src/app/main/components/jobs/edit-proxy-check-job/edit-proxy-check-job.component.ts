import { Component, HostListener } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { faBolt } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { combineLatest, map } from 'rxjs';
import { ProxyCheckJobOptionsDto } from 'src/app/main/dtos/job/proxy-check-job-options.dto';
import { ProxyGroupDto } from 'src/app/main/dtos/proxy-group/proxy-group.dto';
import { OBSettingsDto, ProxyCheckTarget } from 'src/app/main/dtos/settings/ob-settings.dto';
import { JobService } from 'src/app/main/services/job.service';
import { ProxyGroupService } from 'src/app/main/services/proxy-group.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { TimeSpan } from 'src/app/shared/utils/timespan';

enum EditMode {
  Create = 'create',
  Update = 'update',
  Clone = 'clone'
}

enum StartConditionMode {
  Absolute = 'absolute',
  Relative = 'relative'
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

  faBolt = faBolt;

  StartConditionMode = StartConditionMode;

  mode: EditMode = EditMode.Update;
  jobId: number | null = null;
  options: ProxyCheckJobOptionsDto | null = null;
  proxyGroups: ProxyGroupDto[] | null = null;
  settings: OBSettingsDto | null = null;
  targetSiteUrl: string = 'https://example.com';
  targetSiteSuccessKey: string = 'Example Domain';

  startConditionMode: StartConditionMode = StartConditionMode.Absolute;
  startAfter: TimeSpan = new TimeSpan(0);
  startAt: Date = moment().add(1, 'days').toDate();

  defaultProxyGroup = {
    id: -1,
    name: 'All',
    owner: { id: -2, username: 'System' }
  };

  selectedProxyGroup: ProxyGroupDto = this.defaultProxyGroup;

  fieldsValidity: { [key: string] : boolean; } = {};
  touched: boolean = false;

  constructor(
    activatedRoute: ActivatedRoute,
    private proxyGroupService: ProxyGroupService,
    private settingsService: SettingsService,
    private jobService: JobService
  ) {
    combineLatest([activatedRoute.url, activatedRoute.queryParams])
      .subscribe(results => {

        const uriChunks = results[0];

        this.mode = <EditMode>uriChunks[2].path;

        const queryParams = results[1];
        const jobId = queryParams['jobId'];

        if (jobId !== undefined && !isNaN(jobId)) {
          this.jobId = parseInt(jobId);
        }

        this.initJobOptions();
      });

    this.settingsService.getSettings()
      .subscribe(settings => {
        this.settings = settings;
      });

    this.proxyGroupService.getAllProxyGroups()
      .subscribe(proxyGroups => {
        this.proxyGroups = [
          this.defaultProxyGroup,
          ...proxyGroups
        ];
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

        if (options.startCondition._polyTypeName === 'relativeTimeStartCondition') {
          this.startAfter = parseTimeSpan(options.startCondition.startAfter);
          this.startConditionMode = StartConditionMode.Relative;
        } else if (options.startCondition._polyTypeName === 'absoluteTimeStartCondition') {
          this.startAt = moment(options.startCondition.startAt).toDate();
          this.startConditionMode = StartConditionMode.Absolute;
        }
      });
  }

  onValidityChange(validity: FieldValidity) {
    this.fieldsValidity = {
      ...this.fieldsValidity,
      [validity.key]: validity.valid
    };
  }

  targetSiteSelected(target: ProxyCheckTarget) {
    this.targetSiteUrl = target.url;
    this.targetSiteSuccessKey = target.successKey;
  }

  onStartAfterChange(timeSpan: TimeSpan) {
    this.startAfter = timeSpan;
    
    if (this.startConditionMode === StartConditionMode.Relative) {
      this.options!.startCondition = {
        _polyTypeName: 'relativeTimeStartCondition',
        startAfter: this.startAfter.toString()
      };
    }
  }

  // Can accept if touched and every field is valid
  canAccept() {
    return this.touched && Object.values(this.fieldsValidity).every(v => v);
  }

  accept() {
    // TODO: Perform the API calls and redirect to viewer page!
    console.log(this.options);
  }
}
