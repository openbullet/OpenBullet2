import { Component, EventEmitter, Input, Output } from '@angular/core';
import { JobType } from 'src/app/main/dtos/job/job.dto';
import { TriggerType } from 'src/app/main/dtos/monitor/trigger.dto';

@Component({
  selector: 'app-add-trigger',
  templateUrl: './add-trigger.component.html',
  styleUrls: ['./add-trigger.component.scss'],
})
export class AddTriggerComponent {
  @Input() jobType: JobType = JobType.MultiRun;
  @Output() selected = new EventEmitter<TriggerType>();

  type: TriggerType = TriggerType.JobStatus;

  getTriggerTypes(): TriggerType[] {
    let types = [
      TriggerType.JobStatus,
      TriggerType.JobFinished,
      TriggerType.Progress,
      TriggerType.TimeElapsed,
      TriggerType.TimeRemaining,
    ];

    if (this.jobType === JobType.MultiRun) {
      types = types.concat([
        TriggerType.TestedCount,
        TriggerType.HitCount,
        TriggerType.CustomCount,
        TriggerType.ToCheckCount,
        TriggerType.FailCount,
        TriggerType.RetryCount,
        TriggerType.BanCount,
        TriggerType.ErrorCount,
        TriggerType.AliveProxiesCount,
        TriggerType.BannedProxiesCount,
        TriggerType.CpmCount,
        TriggerType.CaptchaCredit,
      ]);
    }

    return types;
  }

  submit(): void {
    this.selected.emit(this.type);
  }
}
