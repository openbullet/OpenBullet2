import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { IconDefinition, faBolt, faDatabase, faEye, faFileLines, faFileShield, faGears, faHouse, faInfo, faPuzzlePiece, faRetweet, faUsers, faWrench } from '@fortawesome/free-solid-svg-icons';

interface MenuSection {
  label: string,
  items: MenuItem[]
}

interface MenuItem {
  icon: IconDefinition,
  label: string,
  link: string
}

interface MenuSection {
  label: string
}

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.scss']
})
export class MenuComponent {
  menu: MenuSection[] = [
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
  ]

  constructor(private router: Router) {

  }

  isItemActive(item: MenuItem): boolean {
    return this.router.url.split('?')[0].startsWith(item.link);
  }
}
