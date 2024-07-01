import { Component, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { faWindowMaximize } from '@fortawesome/free-solid-svg-icons';
import { MRJHitLogDto } from 'src/app/main/dtos/job/hit-log.dto';
import { BotLoggerEntry } from 'src/app/main/models/config-debugger-settings';
import { JobService } from 'src/app/main/services/job.service';
import { ViewAsHtmlComponent } from '../../../config/config-debugger/view-as-html/view-as-html.component';

@Component({
  selector: 'app-hit-log',
  templateUrl: './hit-log.component.html',
  styleUrls: ['./hit-log.component.scss'],
})
export class HitLogComponent {
  @Input() jobId!: number;

  @ViewChild('viewAsHtmlComponent')
  htmlViewer: ViewAsHtmlComponent | undefined = undefined;

  hitLog: MRJHitLogDto | null = null;

  faWindowMaximize = faWindowMaximize;
  viewAsHtmlModalVisible = false;
  html = '';

  constructor(private jobService: JobService) { }

  public getHitLog(hitId: string) {
    this.hitLog = null;

    this.jobService.getHitLog(this.jobId, hitId).subscribe((hitLog) => {
      this.hitLog = hitLog;
    });
  }

  viewAsHtml(entry: BotLoggerEntry) {
    this.html = entry.message;
    this.viewAsHtmlModalVisible = true;
  }
}
