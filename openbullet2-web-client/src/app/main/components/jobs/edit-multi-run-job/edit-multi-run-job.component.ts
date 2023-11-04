import { Component, HostListener, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faBolt, faGears } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Observable, combineLatest } from 'rxjs';
import { ConfigInfoDto } from 'src/app/main/dtos/config/config-info.dto';
import { MultiRunJobOptionsDto } from 'src/app/main/dtos/job/multi-run-job-options.dto';
import { StartConditionMode } from 'src/app/main/dtos/job/start-condition-mode';
import { StartConditionType } from 'src/app/main/dtos/job/start-condition.dto';
import { ProxyGroupDto } from 'src/app/main/dtos/proxy-group/proxy-group.dto';
import { ConfigService } from 'src/app/main/services/config.service';
import { JobService } from 'src/app/main/services/job.service';
import { ProxyGroupService } from 'src/app/main/services/proxy-group.service';
import { SettingsService } from 'src/app/main/services/settings.service';
import { DeactivatableComponent } from 'src/app/shared/guards/can-deactivate-form.guard';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { TimeSpan } from 'src/app/shared/utils/timespan';
import { SelectConfigComponent } from '../select-config/select-config.component';

enum EditMode {
  Create = 'create',
  Edit = 'edit',
  Clone = 'clone'
}

@Component({
  selector: 'app-edit-multi-run-job',
  templateUrl: './edit-multi-run-job.component.html',
  styleUrls: ['./edit-multi-run-job.component.scss']
})
export class EditMultiRunJobComponent implements DeactivatableComponent {
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  @ViewChild('selectConfigComponent') 
  selectConfigComponent: SelectConfigComponent | undefined;

  faBolt = faBolt;
  faGears = faGears;

  StartConditionMode = StartConditionMode;

  mode: EditMode = EditMode.Edit;
  jobId: number | null = null;
  options: MultiRunJobOptionsDto | null = null;
  proxyGroups: ProxyGroupDto[] | null = null;
  
  startConditionMode: StartConditionMode = StartConditionMode.Absolute;
  startAfter: TimeSpan = new TimeSpan(0);
  startAt: Date = moment().add(1, 'days').toDate();

  selectedConfigInfo: ConfigInfoDto | null = null;

  defaultProxyGroup = {
    id: -1,
    name: 'All',
    owner: { id: -2, username: 'System' }
  };

  selectedProxyGroup: ProxyGroupDto = this.defaultProxyGroup;

  fieldsValidity: { [key: string] : boolean; } = {};
  touched: boolean = false;

  selectConfigModalVisible: boolean = false;

  constructor(
    activatedRoute: ActivatedRoute,
    private proxyGroupService: ProxyGroupService,
    private settingsService: SettingsService,
    private jobService: JobService,
    private configService: ConfigService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
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

    this.proxyGroupService.getAllProxyGroups()
      .subscribe(proxyGroups => {
        this.proxyGroups = [
          this.defaultProxyGroup,
          ...proxyGroups
        ];
      });
  }

  canDeactivate() {
    if (!this.touched) {
      return true;
    }

    // Ask for confirmation and return the observable
    return new Observable<boolean>(observer => {
      this.confirmationService.confirm({
        message: `You have unsaved changes. Are you sure that you want to leave?`,
        header: 'Confirmation',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          observer.next(true);
          observer.complete();
        },
        reject: () => {
          observer.next(false);
          observer.complete();
        }
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

    this.jobService.getMultiRunJobOptions(this.jobId ?? -1)
      .subscribe(options => {
        if (options.startCondition._polyTypeName === StartConditionType.Relative) {
          this.startAfter = parseTimeSpan(options.startCondition.startAfter);
          this.startConditionMode = StartConditionMode.Relative;
        } else if (options.startCondition._polyTypeName === StartConditionType.Absolute) {
          this.startAt = moment(options.startCondition.startAt).toDate();
          this.startConditionMode = StartConditionMode.Absolute;
        }

        this.configService.getInfo(options.configId).subscribe(configInfo => {
          this.selectedConfigInfo = configInfo;
        });

        this.options = options;
      });
  }

  onValidityChange(validity: FieldValidity) {
    this.fieldsValidity = {
      ...this.fieldsValidity,
      [validity.key]: validity.valid
    };
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
        startAfter: this.startAfter.toString()
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

    if (this.mode === EditMode.Create) {
      this.jobService.createMultiRunJob(this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Created',
            detail: `Multi run job ${resp.id} was created`
          });
          this.router.navigate([`/job/multi-run/${resp.id}`]);
        });
    } else if (this.mode === EditMode.Edit) {
      this.jobService.updateMultiRunJob(this.jobId!, this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Updated',
            detail: `Multi run job ${resp.id} was updated`
          });
          this.router.navigate([`/job/multi-run/${resp.id}`]);
        });
    } else if (this.mode === EditMode.Clone) {
      this.jobService.createMultiRunJob(this.options)
        .subscribe(resp => {
          this.touched = false;
          this.messageService.add({
            severity: 'success',
            summary: 'Cloned',
            detail: `Multi run job ${resp.id} was cloned from ${this.jobId}`
          });
          this.router.navigate([`/job/multi-run/${resp.id}`]);
        });
    }
  }

  openSelectConfigModal() {
    this.selectConfigModalVisible = true;
    this.selectConfigComponent?.refresh();
  }

  selectConfig(config: ConfigInfoDto) {
    this.selectedConfigInfo = config;
    this.options!.configId = config.id;
    this.selectConfigModalVisible = false;
    this.touched = true;
  }
}
