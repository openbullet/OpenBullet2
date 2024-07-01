import { Component, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import {
  IconDefinition,
  faBolt,
  faCode,
  faDatabase,
  faEye,
  faFileLines,
  faFileShield,
  faGears,
  faGripLines,
  faHouse,
  faInfo,
  faPuzzlePiece,
  faRetweet,
  faSave,
  faTags,
  faUsers,
  faWrench,
} from '@fortawesome/free-solid-svg-icons';
import { MessageService } from 'primeng/api';
import { Subscription } from 'rxjs';
import { ConfigMode } from '../../dtos/config/config-info.dto';
import { ConfigDto } from '../../dtos/config/config.dto';
import { ConfigService } from '../../services/config.service';
import { UserService } from '../../services/user.service';

interface MenuSection {
  label: string;
  items: MenuItem[];
  saveButton: boolean;
  onlyAdmin: boolean;
}

interface MenuItem {
  icon: IconDefinition;
  label: string;
  link: string;
  onlyAdmin: boolean;
}

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss'],
})
export class MenuComponent implements OnDestroy {
  selectedConfigSubscription: Subscription | null = null;
  faSave = faSave;
  selectedConfig: ConfigDto | null = null;

  userRole = 'guest';

  standardMenu: MenuSection[] = [
    {
      label: 'Menu',
      items: [
        {
          icon: faHouse,
          label: 'Home',
          link: '/home',
          onlyAdmin: false,
        },
        {
          icon: faBolt,
          label: 'Jobs',
          link: '/jobs',
          onlyAdmin: false,
        },
        {
          icon: faEye,
          label: 'Monitor',
          link: '/monitor',
          onlyAdmin: true,
        },
        {
          icon: faFileShield,
          label: 'Proxies',
          link: '/proxies',
          onlyAdmin: false,
        },
        {
          icon: faFileLines,
          label: 'Wordlists',
          link: '/wordlists',
          onlyAdmin: false,
        },
        {
          icon: faDatabase,
          label: 'Hits',
          link: '/hits',
          onlyAdmin: false,
        },
        {
          icon: faGears,
          label: 'Configs',
          link: '/configs',
          onlyAdmin: true,
        },
      ],
      saveButton: false,
      onlyAdmin: false,
    },
    {
      label: 'System',
      items: [
        {
          icon: faWrench,
          label: 'Settings',
          link: '/settings',
          onlyAdmin: true,
        },
        {
          icon: faWrench,
          label: 'RL Settings',
          link: '/rl-settings',
          onlyAdmin: true,
        },
      ],
      saveButton: false,
      onlyAdmin: true,
    },
    {
      label: 'More',
      items: [
        {
          icon: faUsers,
          label: 'Guests',
          link: '/guests',
          onlyAdmin: true,
        },
        {
          icon: faPuzzlePiece,
          label: 'Plugins',
          link: '/plugins',
          onlyAdmin: true,
        },
        {
          icon: faRetweet,
          label: 'Sharing',
          link: '/sharing',
          onlyAdmin: true,
        },
        {
          icon: faInfo,
          label: 'Info',
          link: '/info',
          onlyAdmin: false,
        },
      ],
      saveButton: false,
      onlyAdmin: false,
    },
  ];

  menu: MenuSection[] = [];

  constructor(
    private router: Router,
    private messageService: MessageService,
    private configService: ConfigService,
    private userService: UserService,
  ) {
    this.userRole = this.userService.loadUserInfo().role.toLocaleLowerCase();
    this.buildMenu(null);
    this.selectedConfigSubscription = this.configService.selectedConfig$.subscribe((config) => {
      this.buildMenu(config);
      this.selectedConfig = config;
    });

    this.configService.nameChanged$.subscribe(() => {
      this.buildMenu(this.selectedConfig);
    });
  }

  ngOnDestroy(): void {
    this.selectedConfigSubscription?.unsubscribe();
  }

  buildMenu(config: ConfigDto | null) {
    if (config === null) {
      this.menu = this.standardMenu;
      return;
    }

    const menuItems: MenuItem[] = [
      {
        icon: faTags,
        label: 'Metadata',
        link: '/config/metadata',
        onlyAdmin: true,
      },
      {
        icon: faFileLines,
        label: 'Readme',
        link: '/config/readme',
        onlyAdmin: true,
      },
    ];

    if (config.mode === ConfigMode.Stack || config.mode === ConfigMode.LoliCode) {
      menuItems.push(
        {
          icon: faGripLines,
          label: 'Stacker',
          link: '/config/stacker',
          onlyAdmin: true,
        },
        {
          icon: faCode,
          label: 'LoliCode',
          link: '/config/lolicode',
          onlyAdmin: true,
        },
      );
    }

    if (config.mode === ConfigMode.Legacy) {
      menuItems.push({
        icon: faCode,
        label: 'LoliScript',
        link: '/config/loliscript',
        onlyAdmin: true,
      });
    }

    menuItems.push({
      icon: faWrench,
      label: 'Settings',
      link: '/config/settings',
      onlyAdmin: true,
    });

    if (config.mode === ConfigMode.Stack || config.mode === ConfigMode.LoliCode || config.mode === ConfigMode.CSharp) {
      menuItems.push({
        icon: faCode,
        label: 'C# Code',
        link: '/config/csharp',
        onlyAdmin: true,
      });
    }

    this.menu = [
      this.standardMenu[0],
      {
        label: `Config - ${config.metadata.name}`,
        items: menuItems,
        saveButton: true,
        onlyAdmin: true,
      },
      ...this.standardMenu.slice(1),
    ];
  }

  isItemActive(item: MenuItem): boolean {
    return this.router.url.split('?')[0].startsWith(item.link);
  }

  saveConfig() {
    if (this.selectedConfig !== null) {
      this.configService.saveConfig(this.selectedConfig, true).subscribe((c) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Saved',
          detail: `${c.metadata.name} was saved`,
        });
      });
    }
  }

  canView(item: MenuSection | MenuItem): boolean {
    return !item.onlyAdmin || this.userRole === 'admin';
  }
}
