import { Component, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { faEye, faPlus, faSave, faX } from '@fortawesome/free-solid-svg-icons';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Observable, combineLatest } from 'rxjs';
import { JobStatus } from 'src/app/main/dtos/job/job-status';
import { JobOverviewDto, JobType } from 'src/app/main/dtos/job/job.dto';
import { ActionDto, ActionType } from 'src/app/main/dtos/monitor/action.dto';
import { NumComparison, TriggerDto, TriggerType, getComparisonSubject } from 'src/app/main/dtos/monitor/trigger.dto';
import { CreateTriggeredActionDto, UpdateTriggeredActionDto } from 'src/app/main/dtos/monitor/triggered-action.dto';
import { JobMonitorService } from 'src/app/main/services/job-monitor.service';
import { JobService } from 'src/app/main/services/job.service';
import { DeactivatableComponent } from 'src/app/shared/guards/can-deactivate-form.guard';
import { PascalCasePipe } from 'src/app/shared/pipes/pascalcase.pipe';
import { parseTimeSpan } from 'src/app/shared/utils/dates';
import { FieldValidity } from 'src/app/shared/utils/forms';
import { TimeSpan } from 'src/app/shared/utils/timespan';

enum EditMode {
  Create = 'create',
  Edit = 'edit',
  Clone = 'clone',
}

@Component({
  selector: 'app-edit-triggered-action',
  templateUrl: './edit-triggered-action.component.html',
  styleUrls: ['./edit-triggered-action.component.scss'],
})
export class EditTriggeredActionComponent implements DeactivatableComponent {
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  faEye = faEye;
  faSave = faSave;
  faX = faX;
  faPlus = faPlus;

  TriggerType = TriggerType;
  ActionType = ActionType;
  jobStatuses = Object.values(JobStatus);
  numComparisons = Object.values(NumComparison);
  getComparisonSubject = getComparisonSubject;
  parseTimeSpan = parseTimeSpan;

  loaded = false;
  triggeredActionId: string | null = null;
  mode: EditMode = EditMode.Edit;
  jobs: JobOverviewDto[] | null = null;

  name = '';
  isActive = true;
  isRepeatable = false;
  jobId: number | null = null;
  triggers: TriggerDto[] = [];
  actions: ActionDto[] = [];

  fieldsValidity: { [key: string]: boolean } = {};
  touched = false;

  addTriggerModalVisible = false;
  addActionModalVisible = false;

  constructor(
    activatedRoute: ActivatedRoute,
    private router: Router,
    private jobMonitorService: JobMonitorService,
    private jobService: JobService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
  ) {
    combineLatest([activatedRoute.url, activatedRoute.queryParams]).subscribe((results) => {
      const uriChunks = results[0];

      this.mode = <EditMode>uriChunks[2].path;

      const queryParams = results[1];
      const triggeredActionId = queryParams['id'];

      // If we're not creating and we don't have an ID, we can't do anything
      if (triggeredActionId === undefined && this.mode !== EditMode.Create) {
        this.router.navigate(['/monitor']);
        return;
      }

      // Get the jobs
      this.jobService.getAllJobs().subscribe((jobs) => {
        this.jobs = jobs;
      });

      // If we're creating a new triggered action, we don't need to load anything
      if (this.mode === EditMode.Create) {
        this.loaded = true;
        return;
      }

      // If we're editing, we need to set the ID
      if (this.mode === EditMode.Edit) {
        this.triggeredActionId = triggeredActionId;
      }

      // If we're editing or cloning, we need to load the triggered action
      this.jobMonitorService.getTriggeredAction(triggeredActionId).subscribe((triggeredAction) => {
        this.isActive = triggeredAction.isActive;
        this.isRepeatable = triggeredAction.isRepeatable;
        this.jobId = triggeredAction.jobId;
        this.triggers = triggeredAction.triggers;
        this.actions = triggeredAction.actions;

        // Only set the name if we're editing
        if (this.mode === EditMode.Edit) {
          this.name = triggeredAction.name;
        }

        this.loaded = true;
      });
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

  onValidityChange(validity: FieldValidity) {
    this.fieldsValidity = {
      ...this.fieldsValidity,
      [validity.key]: validity.valid,
    };
  }

  // Can accept if touched and every field is valid
  canAccept() {
    return (
      this.touched &&
      Object.values(this.fieldsValidity).every((v) => v) &&
      this.jobId !== null &&
      (this.jobs?.find((j) => j.id === this.jobId) ?? false)
    );
  }

  accept() {
    switch (this.mode) {
      case EditMode.Create:
        this.createTriggeredAction();
        break;
      case EditMode.Edit:
        this.updateTriggeredAction();
        break;
      case EditMode.Clone:
        this.cloneTriggeredAction();
        break;
      default:
        break;
    }
  }

  createTriggeredAction() {
    const createTriggeredActionDto: CreateTriggeredActionDto = {
      name: this.name,
      isActive: this.isActive,
      isRepeatable: this.isRepeatable,
      jobId: this.jobId!,
      triggers: this.triggers,
      actions: this.actions,
    };

    this.jobMonitorService.createTriggeredAction(createTriggeredActionDto).subscribe(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Create',
        detail: `The triggered action ${createTriggeredActionDto.name} was created`,
      });
      this.touched = false;
      this.router.navigate(['/monitor']);
    });
  }

  updateTriggeredAction() {
    const updateTriggeredActionDto: UpdateTriggeredActionDto = {
      id: this.triggeredActionId!,
      name: this.name,
      isActive: this.isActive,
      isRepeatable: this.isRepeatable,
      jobId: this.jobId!,
      triggers: this.triggers,
      actions: this.actions,
    };

    this.jobMonitorService.updateTriggeredAction(updateTriggeredActionDto).subscribe(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Update',
        detail: `The triggered action ${updateTriggeredActionDto.name} was updated`,
      });
      this.touched = false;
      this.router.navigate(['/monitor']);
    });
  }

  cloneTriggeredAction() {
    const createTriggeredActionDto: CreateTriggeredActionDto = {
      name: this.name,
      isActive: this.isActive,
      isRepeatable: this.isRepeatable,
      jobId: this.jobId!,
      triggers: this.triggers,
      actions: this.actions,
    };

    this.jobMonitorService.createTriggeredAction(createTriggeredActionDto).subscribe(() => {
      this.messageService.add({
        severity: 'success',
        summary: 'Clone',
        detail: `The triggered action ${createTriggeredActionDto.name} was cloned`,
      });
      this.touched = false;
      this.router.navigate(['/monitor']);
    });
  }

  getJob(jobId: number | null) {
    if (this.jobId === null || this.jobs === null) {
      return null;
    }

    return this.jobs.find((j) => j.id === jobId) ?? null;
  }

  setMonitoredJob(job: JobOverviewDto | undefined) {
    if (job === undefined) {
      return;
    }

    this.jobId = job.id;
  }

  getJobString(job: JobOverviewDto): string {
    let type = '';

    switch (job.type) {
      case JobType.MultiRun:
        type = 'Multi Run Job';
        break;
      case JobType.ProxyCheck:
        type = 'Proxy Check Job';
        break;
      default:
        break;
    }

    const name = job.name.trim() === '' ? 'Unnamed' : job.name;

    return `#${job.id} | ${name} | ${type}`;
  }

  removeTrigger(index: number) {
    this.triggers.splice(index, 1);
  }

  removeAction(index: number) {
    this.actions.splice(index, 1);
  }

  // biome-ignore lint/suspicious/noExplicitAny: any is used to allow any type of value
  displayEnumValue(value: any): string {
    const pascalPipe = new PascalCasePipe();
    return pascalPipe.transform(value);
  }

  getJobType(): JobType {
    const job = this.getJob(this.jobId);
    return job === null ? JobType.MultiRun : job.type;
  }

  openAddTriggerModal() {
    this.addTriggerModalVisible = true;
  }

  openAddActionModal() {
    this.addActionModalVisible = true;
  }

  createTrigger(type: TriggerType) {
    this.addTriggerModalVisible = false;

    let trigger: TriggerDto;

    switch (type) {
      case TriggerType.JobStatus:
        trigger = {
          _polyTypeName: TriggerType.JobStatus,
          status: JobStatus.RUNNING,
        };
        break;
      case TriggerType.JobFinished:
        trigger = {
          _polyTypeName: TriggerType.JobFinished,
        };
        break;
      case TriggerType.TestedCount:
      case TriggerType.HitCount:
      case TriggerType.CustomCount:
      case TriggerType.ToCheckCount:
      case TriggerType.FailCount:
      case TriggerType.RetryCount:
      case TriggerType.BanCount:
      case TriggerType.ErrorCount:
      case TriggerType.AliveProxiesCount:
      case TriggerType.BannedProxiesCount:
      case TriggerType.CpmCount:
      case TriggerType.CaptchaCredit:
      case TriggerType.Progress:
        trigger = {
          _polyTypeName: type,
          comparison: NumComparison.EqualTo,
          amount: 0,
        };
        break;
      case TriggerType.TimeElapsed:
      case TriggerType.TimeRemaining:
        trigger = {
          _polyTypeName: type,
          comparison: NumComparison.EqualTo,
          timeSpan: TimeSpan.fromSeconds(0).toString(),
        };
        break;
      default:
        return;
    }

    this.triggers.push(trigger);
    this.touched = true;
  }

  createAction(type: ActionType) {
    this.addActionModalVisible = false;

    let action: ActionDto;

    switch (type) {
      case ActionType.Wait:
        action = {
          _polyTypeName: ActionType.Wait,
          timeSpan: TimeSpan.fromSeconds(0).toString(),
        };
        break;
      case ActionType.SetRelativeStartCondition:
        action = {
          _polyTypeName: ActionType.SetRelativeStartCondition,
          jobId: 0,
          timeSpan: TimeSpan.fromSeconds(0).toString(),
        };
        break;
      case ActionType.StopJob:
      case ActionType.AbortJob:
      case ActionType.StartJob:
        action = {
          _polyTypeName: type,
          jobId: 0,
        };
        break;
      case ActionType.DiscordWebhook:
        action = {
          _polyTypeName: ActionType.DiscordWebhook,
          webhook: '',
          message: '',
        };
        break;
      case ActionType.TelegramBot:
        action = {
          _polyTypeName: ActionType.TelegramBot,
          apiServer: 'https://api.telegram.org',
          token: '',
          chatId: 0,
          message: '',
        };
        break;
      case ActionType.SetBots:
        action = {
          _polyTypeName: ActionType.SetBots,
          amount: 0,
        };
        break;
      case ActionType.ReloadProxies:
        action = {
          _polyTypeName: ActionType.ReloadProxies,
        };
        break;
      default:
        return;
    }

    this.actions.push(action);
    this.touched = true;
  }
}
