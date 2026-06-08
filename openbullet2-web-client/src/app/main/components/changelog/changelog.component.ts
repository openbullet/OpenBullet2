import { Component, Input, OnInit } from '@angular/core';
import { ChangelogDto } from '../../dtos/info/changelog.dto';
import { UpdateInfoDto } from '../../dtos/info/update-info.dto';
import { InfoService } from '../../services/info.service';

@Component({
  selector: 'app-changelog',
  templateUrl: './changelog.component.html',
  styleUrls: ['./changelog.component.scss'],
})
export class ChangelogComponent implements OnInit {
  @Input() updateInfo: UpdateInfoDto | null = null;
  changelog: ChangelogDto | null = null;

  constructor(private infoService: InfoService) {}

  ngOnInit(): void {
    this.infoService.getChangelog().subscribe((changelog) => {
      this.changelog = changelog;
    });
  }
}
