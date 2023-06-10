import { Component, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { IconDefinition, faBolt, faCode, faDatabase, faEye, faFileLines, faFileShield, faGears, faGripLines, faHouse, faInfo, faPuzzlePiece, faRetweet, faTags, faUsers, faWrench } from '@fortawesome/free-solid-svg-icons';
import { ConfigService } from '../../services/config.service';
import { Subscription } from 'rxjs';
import { ConfigDto } from '../../dtos/config/config.dto';

interface MenuSection {
  label: string,
  items: MenuItem[]
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
      ]
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
      ]
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
      ]
    }
  ];

  menu: MenuSection[] = [];

  constructor(private router: Router,
    private configService: ConfigService) {
      this.buildMenu(null);
      this.selectedConfigSubscription = this.configService.selectedConfig$
        .subscribe(config => {
          this.buildMenu(config);
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
        label: config.metadata.name,
        items: menuItems
      },
      ...this.standardMenu.slice(1)
    ]
  }

  isItemActive(item: MenuItem): boolean {
    return this.router.url.split('?')[0].startsWith(item.link);
  }
}
