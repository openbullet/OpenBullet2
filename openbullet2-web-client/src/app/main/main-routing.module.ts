import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainComponent } from './main.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { HomeComponent } from './components/home/home.component';
import { InfoComponent } from './components/info/info.component';
import { GuestsComponent } from './components/guests/guests.component';
import { PluginsComponent } from './components/plugins/plugins.component';
import { SharingComponent } from './components/sharing/sharing.component';
import { OBSettingsComponent } from './components/ob-settings/ob-settings.component';
import { RlSettingsComponent } from './components/rl-settings/rl-settings.component';
import { ProxiesComponent } from './components/proxies/proxies.component';
import { HitsComponent } from './components/hits/hits.component';
import { WordlistsComponent } from './components/wordlists/wordlists.component';
import { ConfigsComponent } from './components/configs/configs.component';
import { ConfigMetadataComponent } from './components/config/config-metadata/config-metadata.component';
import { ConfigReadmeComponent } from './components/config/config-readme/config-readme.component';
import { ConfigSettingsComponent } from './components/config/config-settings/config-settings.component';
import { ConfigLolicodeComponent } from './components/config/config-lolicode/config-lolicode.component';

const routes: Routes = [
  // Main component layout
  {
    path: '',
    component: MainComponent,
    children: [
      {
        component: HomeComponent,
        path: 'home'
      },
      {
        component: ProxiesComponent,
        path: 'proxies'
      },
      {
        component: WordlistsComponent,
        path: 'wordlists'
      },
      {
        component: HitsComponent,
        path: 'hits'
      },
      {
        component: ConfigsComponent,
        path: 'configs'
      },
      {
        component: ConfigMetadataComponent,
        path: 'config/metadata'
      },
      {
        component: ConfigReadmeComponent,
        path: 'config/readme'
      },
      {
        component: ConfigLolicodeComponent,
        path: 'config/lolicode'
      },
      {
        component: ConfigSettingsComponent,
        path: 'config/settings'
      },
      {
        component: OBSettingsComponent,
        path: 'settings'
      },
      {
        component: RlSettingsComponent,
        path: 'rl-settings'
      },
      {
        component: GuestsComponent,
        path: 'guests'
      },
      {
        component: PluginsComponent,
        path: 'plugins'
      },
      {
        component: SharingComponent,
        path: 'sharing'
      },
      {
        component: InfoComponent,
        path: 'info'
      }
    ]
  },
  {
    path: '**',
    component: NotFoundComponent,
    pathMatch: 'full'
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class MainRoutingModule { }
