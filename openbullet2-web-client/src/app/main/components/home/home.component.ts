import { Component, OnInit } from '@angular/core';
import { MarkdownService } from 'ngx-markdown';
import { InfoService } from '../../services/info.service';
import { AnnouncementDto } from '../../dtos/info/announcement.dto';
import { ServerInfoDto } from '../../dtos/info/server-info.dto';
import { faCopy } from '@fortawesome/free-solid-svg-icons';
import { TimeSpan } from 'src/app/shared/utils/timespan';
import * as moment from 'moment';
import { Moment } from 'moment';
import { addTimeSpan, parseTimeSpan } from 'src/app/shared/utils/dates';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  announcement: AnnouncementDto | null = null;
  serverInfo: ServerInfoDto | null = null;
  serverOffset: TimeSpan | null = null;
  serverStartTime: Moment | null = null;
  serverUptime: TimeSpan | null = null;
  serverTime: Moment | null = null;
  buildDate: Moment | null = null;
  faCopy = faCopy;

  constructor(
    private markdownService: MarkdownService,
    private infoService: InfoService) {  
  }

  ngOnInit(): void {
    this.infoService.getAnnouncement()
      .subscribe(ann => this.announcement = ann);

    this.infoService.getServerInfo()
      .subscribe(info => {
        this.serverStartTime = moment(info.startTime);
        this.serverOffset = parseTimeSpan(info.localUtcOffset);
        this.buildDate = moment(info.buildDate);
        this.serverInfo = info;

        this.updateTimes();
        setInterval(() => {
          this.updateTimes();
        }, 1000);
      });
  }

  updateTimes() {
    if (this.serverInfo === null) return;
    if (this.serverStartTime === null) return;
    if (this.serverOffset === null) return;

    this.serverTime = addTimeSpan(moment(), this.serverOffset);
    const uptimeDiff = this.serverTime.diff(this.serverStartTime);
    this.serverUptime = new TimeSpan(uptimeDiff);
  }

  copyCurrentWorkingDirectory() {
    if (this.serverInfo === null) return;
    navigator.clipboard.writeText(this.serverInfo.currentWorkingDirectory);
  }
}
