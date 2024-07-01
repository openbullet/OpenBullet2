import { Component, EventEmitter, Input, Output } from '@angular/core';
import { JobType } from 'src/app/main/dtos/job/job.dto';
import { ActionType } from 'src/app/main/dtos/monitor/action.dto';

@Component({
  selector: 'app-add-action',
  templateUrl: './add-action.component.html',
  styleUrls: ['./add-action.component.scss'],
})
export class AddActionComponent {
  @Input() jobType: JobType = JobType.MultiRun;
  @Output() selected = new EventEmitter<ActionType>();

  type: ActionType = ActionType.Wait;

  getActionTypes(): ActionType[] {
    let types = [
      ActionType.Wait,
      ActionType.SetRelativeStartCondition,
      ActionType.StopJob,
      ActionType.AbortJob,
      ActionType.StartJob,
      ActionType.DiscordWebhook,
      ActionType.TelegramBot,
    ];

    if (this.jobType === JobType.MultiRun) {
      types = types.concat([ActionType.SetBots, ActionType.ReloadProxies]);
    }

    return types;
  }

  submit(): void {
    this.selected.emit(this.type);
  }
}
