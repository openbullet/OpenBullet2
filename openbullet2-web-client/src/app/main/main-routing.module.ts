import { NgModule, inject } from '@angular/core';
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
import { ConfigCsharpComponent } from './components/config/config-csharp/config-csharp.component';
import { updateCSharpScript } from './utils/config-conversion';
import { AdminGuard } from '../shared/guards/admin.guard';
import { JobsComponent } from './components/jobs/jobs.component';
import { EditProxyCheckJobComponent } from './components/jobs/edit-proxy-check-job/edit-proxy-check-job.component';
import { EditMultiRunJobComponent } from './components/jobs/edit-multi-run-job/edit-multi-run-job.component';

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
        component: JobsComponent,
        path: 'jobs'
      },
      {
        component: EditMultiRunJobComponent,
        path: 'jobs/multi-run/edit'
      },
      {
        component: EditMultiRunJobComponent,
        path: 'jobs/multi-run/create'
      },
      {
        component: EditMultiRunJobComponent,
        path: 'jobs/multi-run/clone'
      },
      {
        component: EditProxyCheckJobComponent,
        path: 'jobs/proxy-check/edit'
      },
      {
        component: EditProxyCheckJobComponent,
        path: 'jobs/proxy-check/create'
      },
      {
        component: EditProxyCheckJobComponent,
        path: 'jobs/proxy-check/clone'
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
        path: 'configs',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: ConfigMetadataComponent,
        path: 'config/metadata',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: ConfigReadmeComponent,
        path: 'config/readme',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: ConfigLolicodeComponent,
        path: 'config/lolicode',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: ConfigSettingsComponent,
        path: 'config/settings',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: ConfigCsharpComponent,
        path: 'config/csharp',
        resolve: { data: updateCSharpScript },
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: OBSettingsComponent,
        path: 'settings',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: RlSettingsComponent,
        path: 'rl-settings',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: GuestsComponent,
        path: 'guests',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: PluginsComponent,
        path: 'plugins',
        canActivate: [() => inject(AdminGuard).canActivate()]
      },
      {
        component: SharingComponent,
        path: 'sharing',
        canActivate: [() => inject(AdminGuard).canActivate()]
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
