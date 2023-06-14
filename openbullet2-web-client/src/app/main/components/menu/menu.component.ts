import { Component, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { IconDefinition, faBolt, faCode, faDatabase, faEye, faFileLines, faFileShield, faGears, faGripLines, faHouse, faInfo, faPuzzlePiece, faRetweet, faSave, faTags, faUsers, faWrench } from '@fortawesome/free-solid-svg-icons';
import { ConfigService } from '../../services/config.service';
import { Subscription } from 'rxjs';
import { ConfigDto } from '../../dtos/config/config.dto';
import { MessageService } from 'primeng/api';

interface MenuSection {
  label: string,
  items: MenuItem[],
  saveButton: boolean
}

interface MenuItem {
  icon: IconDefinition,
  label: string,
  link: string
}

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent implements OnDestroy {
  selectedConfigSubscription: Subscription | null = null;
  faSave = faSave;
  selectedConfig: ConfigDto | null = null;

  standardMenu: MenuSection[] = [
    {
      label: 'Menu',
      items: [
        {
          icon: faHouse,
          label: 'Home',
          link: '/home'
        },
        {
          icon: faBolt,
          label: 'Jobs',
          link: '/jobs'
        },
        {
          icon: faEye,
          label: 'Monitor',
          link: '/monitor'
        },
        {
          icon: faFileShield,
          label: 'Proxies',
          link: '/proxies'
        },
        {
          icon: faFileLines,
          label: 'Wordlists',
          link: '/wordlists'
        },
        {
          icon: faDatabase,
          label: 'Hits',
          link: '/hits'
        },
        {
          icon: faGears,
          label: 'Configs',
          link: '/configs'
        }
      ],
      saveButton: false
    },
    {
      label: 'Configuration',
      items: [
        {
          icon: faWrench,
          label: 'Settings',
          link: '/settings'
        },
        {
          icon: faWrench,
          label: 'RL Settings',
          link: '/rl-settings'
        }
      ],
      saveButton: false
    },
    {
      label: 'More',
      items: [
        {
          icon: faUsers,
          label: 'Guests',
          link: '/guests'
        },
        {
          icon: faPuzzlePiece,
          label: 'Plugins',
          link: '/plugins'
        },
        {
          icon: faRetweet,
          label: 'Sharing',
          link: '/sharing'
        },
        {
          icon: faInfo,
          label: 'Info',
          link: '/info'
        }
      ],
      saveButton: false
    }
  ];

  menu: MenuSection[] = [];

  constructor(private router: Router,
    private messageService: MessageService,
    private configService: ConfigService) {
      this.buildMenu(null);
      this.selectedConfigSubscription = this.configService.selectedConfig$
        .subscribe(config => {
          this.buildMenu(config);
          this.selectedConfig = config;
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
        link: '/config/metadata'
      },
      {
        icon: faFileLines,
        label: 'Readme',
        link: '/config/readme'
      }
    ];

    if (config.mode === 'stack' || config.mode === 'loliCode') {
      menuItems.push(
        {
          icon: faGripLines,
          label: 'Stacker',
          link: '/config/stacker'
        },
        {
          icon: faCode,
          label: 'LoliCode',
          link: '/config/lolicode'
        }
      );
    }

    if (config.mode === 'legacy') {
      menuItems.push(
        {
          icon: faCode,
          label: 'LoliScript',
          link: '/config/loliscript'
        }
      );
    }

    menuItems.push(
      {
        icon: faWrench,
        label: 'Settings',
        link: '/config/settings'
      }
    );

    if (config.mode === 'stack' || config.mode === 'loliCode' 
    || config.mode === 'cSharp') {
      menuItems.push(
        {
          icon: faCode,
          label: 'C# Code',
          link: '/config/csharp'
        }
      );
    }

    this.menu = [
      this.standardMenu[0],
      {
        label: 'Config - ' + config.metadata.name,
        items: menuItems,
        saveButton: true
      },
      ...this.standardMenu.slice(1)
    ]
  }

  isItemActive(item: MenuItem): boolean {
    return this.router.url.split('?')[0].startsWith(item.link);
  }

  saveConfig() {
    if (this.selectedConfig !== null) {
      this.configService.saveConfig(this.selectedConfig)
        .subscribe(c => {
          this.messageService.add({
            severity: 'success',
            summary: 'Saved',
            detail: `${c.metadata.name} was saved`
          });
        });
    }
  }
}
