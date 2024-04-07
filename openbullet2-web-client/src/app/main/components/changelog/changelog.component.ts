import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { faArrowRight, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';
import { ChangelogDto } from '../../dtos/info/changelog.dto';
import { UpdateInfoDto } from '../../dtos/info/update-info.dto';
import { InfoService } from '../../services/info.service';

@Component({
  selector: 'app-changelog',
  templateUrl: './changelog.component.html',
  styleUrls: ['./changelog.component.scss'],
})
export class ChangelogComponent implements OnChanges {
  @Input() updateInfo: UpdateInfoDto | null = null;
  changelog: ChangelogDto | null = null;
  newChangelog: ChangelogDto | null = null;
  faExclamationTriangle = faExclamationTriangle;
  faArrowRight = faArrowRight;

  constructor(private infoService: InfoService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (this.updateInfo === null) {
      return;
    }

    if (this.updateInfo.isUpdateAvailable) {
      if (this.isStagingBuild(this.updateInfo.remoteVersion)) {
        this.newChangelog = {
          version: this.updateInfo.remoteVersion,
          markdownText: 'Staging build, no changelog available.',
        };
      } else {
        this.infoService.getChangelog(this.updateInfo.remoteVersion).subscribe((changelog) => {
          this.newChangelog = changelog;
        });
      }
    }

    if (this.isStagingBuild(this.updateInfo.currentVersion)) {
      this.changelog = {
        version: this.updateInfo.currentVersion,
        markdownText: 'Staging build, no changelog available.',
      };
    } else {
      this.infoService.getChangelog(this.updateInfo.currentVersion ?? null).subscribe((changelog) => {
        this.changelog = changelog;
      });
    }
  }

  isStagingBuild(version: string): boolean {
    return version.split('.').length === 4;
  }
}
