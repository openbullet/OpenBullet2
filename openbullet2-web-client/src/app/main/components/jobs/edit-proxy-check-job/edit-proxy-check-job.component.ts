import { Component, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faBolt, faSave } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Observable, combineLatest } from 'rxjs';
import { ProxyCheckJobOptionsDto } from 'src/app/main/dtos/job/proxy-check-job-options.dto';
import { ProxyCheckTargetDto } from 'src/app/main/dtos/job/proxy-check-job.dto';
import { StartConditionMode } from 'src/app/main/dtos/job/start-condition-mode';
import { StartConditionType } from 'src/app/main/dtos/job/start-condition.dto';
import { ProxyGroupDto } from 'src/app/main/dtos/proxy-group/proxy-group.dto';
import { OBSettingsDto, ProxyCheckTarget } from 'src/app/main/dtos/settings/ob-settings.dto';
import { JobService } from 'src/app/main/services/job.service';
import { ProxyGroupService } from 'src/app/main/services/proxy-group.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { UserService } from 'src/app/main/services/user.service';
import { DeactivatableComponent } from 'src/app/shared/guards/can-deactivate-form.guard';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { TimeSpan } from 'src/app/shared/utils/timespan';

enum EditMode {
  Create = 'create',
  Edit = 'edit',
  Clone = 'clone',
}

@Component({
  selector: 'app-edit-proxy-check-job',
  templateUrl: './edit-proxy-check-job.component.html',
  styleUrls: ['./edit-proxy-check-job.component.scss'],
})
export class EditProxyCheckJobComponent implements DeactivatableComponent {
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  faBolt = faBolt;
  faSave = faSave;

  StartConditionMode = StartConditionMode;

  mode: EditMode = EditMode.Edit;
  jobId: number | null = null;
  options: ProxyCheckJobOptionsDto | null = null;
  proxyGroups: ProxyGroupDto[] | null = null;
  settings: OBSettingsDto | null = null;
  proxyCheckTargets: ProxyCheckTarget[] = [
    {
      url: 'Custom',
      successKey: '',
    },
  ];
  targetSiteUrl = 'https://example.com';
  targetSiteSuccessKey = 'Example Domain';

  startConditionMode: StartConditionMode = StartConditionMode.Absolute;
  startAfter: TimeSpan = new TimeSpan(0);
  startAt: Date = moment().add(1, 'days').toDate();
  botLimit = 200;

  defaultProxyGroup = {
    id: -1,
    name: 'All',
    owner: { id: -2, username: 'System' },
  };

  selectedProxyGroup: ProxyGroupDto = this.defaultProxyGroup;

  fieldsValidity: { [key: string]: boolean } = {};
  touched = false;

  constructor(
    activatedRoute: ActivatedRoute,
    settingsService: SettingsService,
    private proxyGroupService: ProxyGroupService,
    private jobService: JobService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router,
    private userService: UserService,
  ) {
    combineLatest([activatedRoute.url, activatedRoute.queryParams]).subscribe((results) => {
      const uriChunks = results[0];

      this.mode = <EditMode>uriChunks[2].path;

      const queryParams = results[1];
      const jobId = queryParams['jobId'];

      if (jobId !== undefined && !Number.isNaN(jobId)) {
        this.jobId = Number.parseInt(jobId);
      }

      this.initJobOptions();
    });

    if (this.userService.isAdmin()) {
      settingsService.getSettings().subscribe((settings) => {
        this.settings = settings;
        this.proxyCheckTargets = [
          {
            url: 'Custom',
            successKey: '',
          },
          ...settings.generalSettings.proxyCheckTargets,
        ];
      });
    }

    this.proxyGroupService.getAllProxyGroups().subscribe((proxyGroups) => {
      this.proxyGroups = [this.defaultProxyGroup, ...proxyGroups];
    });

    settingsService.getSystemSettings().subscribe((settings) => {
      this.botLimit = settings.botLimit;
    });
  }

  canDeactivate() {
    if (!this.touched) {
      return true;
    }

    // Ask for confirmation and return the observable
    return new Observable<boolean>((observer) => {
      this.confirmationService.confirm({
        message: 'You have unsaved changes. Are you sure that you want to leave?',
        header: 'Confirmation',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          observer.next(true);
          observer.complete();
        },
        reject: () => {
          observer.next(false);
          observer.complete();
        },
      });
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

    this.jobService.getProxyCheckJobOptions(this.jobId ?? -1).subscribe((options) => {
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
      [validity.key]: validity.valid,
    };
  }

  targetSiteSelected(target: ProxyCheckTarget) {
    if (target.url === 'Custom') {
      return;
    }
    this.targetSiteUrl = target.url;
    this.targetSiteSuccessKey = target.successKey;
    this.touched = true;
  }

  onStartConditionModeChange(mode: StartConditionMode) {
    this.startConditionMode = mode;
    this.touched = true;
  }

  onStartAfterChange(timeSpan: TimeSpan) {
    this.startAfter = timeSpan;

    if (this.startConditionMode === StartConditionMode.Relative) {
      this.options!.startCondition = {
        _polyTypeName: StartConditionType.Relative,
        startAfter: this.startAfter.toString(),
      };
    }
  }

  onStartAtChange(date: Date) {
    // Convert to local time, otherwise it's UTC
    this.startAt = moment(date).local().toDate();
    this.touched = true;

    if (this.startConditionMode === StartConditionMode.Absolute) {
      this.options!.startCondition = {
        _polyTypeName: StartConditionType.Absolute,
        startAt: this.startAt.toISOString(),
      };
    }
  }

  // Can accept if touched and every field is valid
  canAccept() {
    return Object.values(this.fieldsValidity).every((v) => v);
  }

  accept() {
    if (this.options === null) {
      return;
    }

    this.options.target = <ProxyCheckTargetDto>{
      url: this.targetSiteUrl,
      successKey: this.targetSiteSuccessKey,
    };

    if (this.mode === EditMode.Create) {
      this.jobService.createProxyCheckJob(this.options).subscribe((resp) => {
        this.touched = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Created',
          detail: `Proxy check job ${resp.id} was created`,
        });
        this.router.navigate([`/job/proxy-check/${resp.id}`]);
      });
    } else if (this.mode === EditMode.Edit) {
      this.jobService.updateProxyCheckJob(this.jobId!, this.options).subscribe((resp) => {
        this.touched = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Updated',
          detail: `Proxy check job ${resp.id} was updated`,
        });
        this.router.navigate([`/job/proxy-check/${resp.id}`]);
      });
    } else if (this.mode === EditMode.Clone) {
      this.jobService.createProxyCheckJob(this.options).subscribe((resp) => {
        this.touched = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Cloned',
          detail: `Proxy check job ${resp.id} was cloned from ${this.jobId}`,
        });
        this.router.navigate([`/job/proxy-check/${resp.id}`]);
      });
    }
  }
}
