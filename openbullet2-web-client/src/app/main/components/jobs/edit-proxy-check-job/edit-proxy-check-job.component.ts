import { Component, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faBolt } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { MessageService } from 'primeng/api';
import { combineLatest, map } from 'rxjs';
import { ProxyCheckJobOptionsDto } from 'src/app/main/dtos/job/proxy-check-job-options.dto';
import { ProxyCheckTargetDto } from 'src/app/main/dtos/job/proxy-check-job.dto';
import { StartConditionType } from 'src/app/main/dtos/job/start-condition.dto';
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
  Edit = 'edit',
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

  mode: EditMode = EditMode.Edit;
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
    private jobService: JobService,
    private messageService: MessageService,
    private router: Router
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
        if (options.target !== null) {
          this.targetSiteUrl = options.target.url;
          this.targetSiteSuccessKey = options.target.successKey;
        }

        if (options.startCondition._polyTypeName === StartConditionType.Relative) {
          this.startAfter = parseTimeSpan(options.startCondition.startAfter);
          this.startConditionMode = StartConditionMode.Relative;
        } else if (options.startCondition._polyTypeName === StartConditionType.Absolute) {
          this.startAt = moment(options.startCondition.startAt).toDate();
          this.startConditionMode = StartConditionMode.Absolute;
        }

        this.options = options;
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
    this.touched = true;
  }

  onStartAfterChange(timeSpan: TimeSpan) {
    this.startAfter = timeSpan;
    
    if (this.startConditionMode === StartConditionMode.Relative) {
      this.options!.startCondition = {
        _polyTypeName: StartConditionType.Relative,
        startAfter: this.startAfter.toString()
      };
    }
  }

  onStartAtChange(date: Date) {
    // Convert to local time, otherwise it's UTC
    this.startAt = moment(date).local().toDate();

    if (this.startConditionMode === StartConditionMode.Absolute) {
      this.options!.startCondition = {
        _polyTypeName: StartConditionType.Absolute,
        startAt: this.startAt.toISOString()
      };
    }
  }

  // Can accept if touched and every field is valid
  canAccept() {
    return this.touched && Object.values(this.fieldsValidity).every(v => v);
  }

  accept() {
    if (this.options === null) {
      return;
    }

    this.options.target = <ProxyCheckTargetDto>{
      url: this.targetSiteUrl,
      successKey: this.targetSiteSuccessKey
    };

    if (this.mode === EditMode.Create) {
      this.jobService.createProxyCheckJob(this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Created',
            detail: `Proxy check job ${resp.id} was created`
          });
          this.router.navigate([`/job/proxy-check/${resp.id}`]);
        });
    } else if (this.mode === EditMode.Edit) {
      this.jobService.updateProxyCheckJob(this.jobId!, this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Updated',
            detail: `Proxy check job ${resp.id} was updated`
          });
          this.router.navigate([`/job/proxy-check/${resp.id}`]);
        });
    } else if (this.mode === EditMode.Clone) {
      this.jobService.createProxyCheckJob(this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Cloned',
            detail: `Proxy check job ${resp.id} was cloned from ${this.jobId}`
          });
          this.router.navigate([`/job/proxy-check/${resp.id}`]);
        });
    }
  }
}
