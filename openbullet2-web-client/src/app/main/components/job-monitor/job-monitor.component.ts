import { Component, OnInit } from '@angular/core';
import { faEye } from '@fortawesome/free-solid-svg-icons';
import { TriggeredActionDto } from '../../dtos/monitor/triggered-action.dto';
import { JobMonitorService } from '../../services/job-monitor.service';
import { JobService } from '../../services/job.service';
import { JobOverviewDto } from '../../dtos/job/job.dto';

@Component({
  selector: 'app-job-monitor',
  templateUrl: './job-monitor.component.html',
  styleUrls: ['./job-monitor.component.scss']
})
export class JobMonitorComponent implements OnInit {
  faEye = faEye;
  triggeredActions: TriggeredActionDto[] | null = null;

  constructor(
    private jobMonitorService: JobMonitorService
  ) { }

  ngOnInit(): void {
    this.jobMonitorService.getAllTriggeredActions().subscribe(
      (triggeredActions) => {
        this.triggeredActions = triggeredActions;
      }
    );
  }
}
