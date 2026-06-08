import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { faCaretLeft, faCaretRight, faExclamationTriangle, faRightFromBracket } from '@fortawesome/free-solid-svg-icons';
import { UpdateInfoDto } from './dtos/info/update-info.dto';
import { InfoService } from './services/info.service';
import { UserService } from './services/user.service';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss'],
})
export class MainComponent implements OnInit {
  updateInfo: UpdateInfoDto | null = null;
  sidebarVisible = true;
  faCaretLeft = faCaretLeft;
  faCaretRight = faCaretRight;
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

  toggleSidebar() {
    this.sidebarVisible = !this.sidebarVisible;
  }
}
