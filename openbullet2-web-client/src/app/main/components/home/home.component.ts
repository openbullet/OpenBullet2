import { Component, OnDestroy, OnInit } from '@angular/core';
import { faCopy, faRightFromBracket } from '@fortawesome/free-solid-svg-icons';
import * as moment from 'moment';
import { Moment } from 'moment';
import { addTimeSpan, parseTimeSpan } from 'src/app/shared/utils/dates';
import { TimeSpan } from 'src/app/shared/utils/timespan';
import { RecentHitsDto } from '../../dtos/hit/recent-hits.dto';
import { AnnouncementDto } from '../../dtos/info/announcement.dto';
import { CollectionInfoDto } from '../../dtos/info/collection-info.dto';
import { ServerInfoDto } from '../../dtos/info/server-info.dto';
import { HitService } from '../../services/hit.service';
import { InfoService } from '../../services/info.service';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit, OnDestroy {
  announcement: AnnouncementDto | null = null;
  collectionInfo: CollectionInfoDto | null = null;
  serverInfo: ServerInfoDto | null = null;
  serverOffset: TimeSpan | null = null;
  serverStartTime: Moment | null = null;
  serverUptime: TimeSpan | null = null;
  serverTime: Moment | null = null;
  buildDate: Date | null = null;
  faCopy = faCopy;
  // biome-ignore lint/suspicious/noExplicitAny: Timer
  timer: any;
  // biome-ignore lint/suspicious/noExplicitAny: Timer
  perfTimer: any;
  username = 'admin';
  recentHits: RecentHitsDto | null = null;

  // Performance
  cpuUsage = '0.00%';
  cpuUsageData: number[] = [];
  memoryUsage = '0 B';
  memoryUsageData: number[] = [];
  networkUsage = '0 B/s | 0 B/s';
  networkUsageData: number[] = [];

  constructor(
    private infoService: InfoService,
    private hitService: HitService,
    private userService: UserService,
  ) { }

  // Clear the timer when navigating off the page
  ngOnDestroy(): void {
    clearInterval(this.timer);
    clearInterval(this.perfTimer);
  }

  ngOnInit(): void {
    this.username = this.userService.loadUserInfo().username;

    this.infoService.getAnnouncement().subscribe((ann) => {
      this.announcement = ann;
    });

    this.infoService.getCollectionInfo().subscribe((coll) => {
      this.collectionInfo = coll;
    });

    this.infoService.getServerInfo().subscribe((info) => {
      this.serverStartTime = moment(info.startTime);
      this.serverOffset = parseTimeSpan(info.localUtcOffset);
      this.buildDate = info.buildDate;
      this.serverInfo = info;

      this.updateTimes();
      this.timer = setInterval(() => {
        this.updateTimes();
      }, 1000);
    });

    this.hitService.getRecentHits(7).subscribe((hits) => {
      this.recentHits = hits;
    });
  }

  updateTimes() {
    if (this.serverInfo === null) return;
    if (this.serverStartTime === null) return;
    if (this.serverOffset === null) return;

    this.serverTime = addTimeSpan(moment().utc(), this.serverOffset);
    const uptimeDiff = moment().utc().diff(this.serverStartTime);
    this.serverUptime = new TimeSpan(uptimeDiff);
  }

  copyCurrentWorkingDirectory() {
    if (this.serverInfo === null) return;
    navigator.clipboard.writeText(this.serverInfo.currentWorkingDirectory);
  }
}
