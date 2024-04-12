import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { faExclamationTriangle, faRightFromBracket } from '@fortawesome/free-solid-svg-icons';
import { UpdateInfoDto, VersionType } from './dtos/info/update-info.dto';
import { InfoService } from './services/info.service';
import { UserService } from './services/user.service';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss'],
})
export class MainComponent implements OnInit {
  updateInfo: UpdateInfoDto | null = null;
  faExclamationTriangle = faExclamationTriangle;
  faRightFromBracket = faRightFromBracket;
  changelogModalVisible = false;

  constructor(
    private router: Router,
    private infoService: InfoService,
    private userService: UserService,
  ) { }

  ngOnInit(): void {
    if (window.location.pathname === '/') {
      this.router.navigate(['home']);
      return;
    }

    this.infoService.getUpdateInfo().subscribe((updateInfo) => {
      this.updateInfo = updateInfo;
    });

    // Mock update info
    // this.updateInfo = {
    //   currentVersion: '0.2.4',
    //   currentVersionType: VersionType.Beta,
    //   remoteVersion: '0.2.5',
    //   remoteVersionType: VersionType.Beta,
    //   isUpdateAvailable: true
    // };

    // Every 12 hours, check for updates
    setInterval(
      () => {
        this.infoService.getUpdateInfo().subscribe((updateInfo) => {
          this.updateInfo = updateInfo;
        });
      },
      12 * 60 * 60 * 1000,
    );
  }

  showChangelog() {
    this.changelogModalVisible = true;
  }

  canLogout(): boolean {
    return this.userService.getJwt() !== null;
  }

  logout() {
    this.userService.resetJwt();
    window.location.reload();
  }
}
