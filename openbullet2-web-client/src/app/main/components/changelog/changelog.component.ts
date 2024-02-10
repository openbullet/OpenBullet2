import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { UpdateInfoDto } from '../../dtos/info/update-info.dto';
import { InfoService } from '../../services/info.service';
import { ChangelogDto } from '../../dtos/info/changelog.dto';
import { faArrowRight, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-changelog',
  templateUrl: './changelog.component.html',
  styleUrls: ['./changelog.component.scss']
})
export class ChangelogComponent implements OnChanges {
  @Input() updateInfo: UpdateInfoDto | null = null;
  changelog: ChangelogDto | null = null;
  newChangelog: ChangelogDto | null = null;
  faExclamationTriangle = faExclamationTriangle;
  faArrowRight = faArrowRight;

  constructor(private infoService: InfoService) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.updateInfo?.isUpdateAvailable) {
      this.infoService.getChangelog(this.updateInfo!.remoteVersion).subscribe(
        (changelog) => {
          this.newChangelog = changelog;
        }
      );
    }

    this.infoService.getChangelog(this.updateInfo?.currentVersion ?? null).subscribe(
      (changelog) => {
        this.changelog = changelog;
      }
    );
  }
}
