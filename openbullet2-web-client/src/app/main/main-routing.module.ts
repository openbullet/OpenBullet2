import { NgModule, inject } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AdminGuard } from '../shared/guards/admin.guard';
import { canDeactivateFormComponent } from '../shared/guards/can-deactivate-form.guard';
import { ConfigCsharpComponent } from './components/config/config-csharp/config-csharp.component';
import { ConfigLolicodeComponent } from './components/config/config-lolicode/config-lolicode.component';
import { ConfigLoliscriptComponent } from './components/config/config-loliscript/config-loliscript.component';
import { ConfigMetadataComponent } from './components/config/config-metadata/config-metadata.component';
import { ConfigReadmeComponent } from './components/config/config-readme/config-readme.component';
import { ConfigSettingsComponent } from './components/config/config-settings/config-settings.component';
import { ConfigStackerComponent } from './components/config/config-stacker/config-stacker.component';
import { ConfigsComponent } from './components/configs/configs.component';
import { GuestsComponent } from './components/guests/guests.component';
import { HitsComponent } from './components/hits/hits.component';
import { HomeComponent } from './components/home/home.component';
import { InfoComponent } from './components/info/info.component';
import { EditTriggeredActionComponent } from './components/job-monitor/edit-triggered-action/edit-triggered-action.component';
import { JobMonitorComponent } from './components/job-monitor/job-monitor.component';
import { EditMultiRunJobComponent } from './components/jobs/edit-multi-run-job/edit-multi-run-job.component';
import { EditProxyCheckJobComponent } from './components/jobs/edit-proxy-check-job/edit-proxy-check-job.component';
import { JobsComponent } from './components/jobs/jobs.component';
import { MultiRunJobComponent } from './components/jobs/multi-run-job/multi-run-job.component';
import { ProxyCheckJobComponent } from './components/jobs/proxy-check-job/proxy-check-job.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { OBSettingsComponent } from './components/ob-settings/ob-settings.component';
import { PluginsComponent } from './components/plugins/plugins.component';
import { ProxiesComponent } from './components/proxies/proxies.component';
import { RlSettingsComponent } from './components/rl-settings/rl-settings.component';
import { SharingComponent } from './components/sharing/sharing.component';
import { WordlistsComponent } from './components/wordlists/wordlists.component';
import { MainComponent } from './main.component';
import { updateCSharpScript } from './utils/config-conversion';
import { ErrorDetailsComponent } from './components/error-details/error-details.component';

const routes: Routes = [
  // Main component layout
  {
    path: '',
    component: MainComponent,
    children: [
      {
        component: HomeComponent,
        path: 'home',
      },
      {
        component: JobsComponent,
        path: 'jobs',
      },
      {
        component: EditMultiRunJobComponent,
        path: 'job/multi-run/edit',
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: EditMultiRunJobComponent,
        path: 'job/multi-run/create',
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: EditMultiRunJobComponent,
        path: 'job/multi-run/clone',
        canDeactivate: [canDeactivateFormComponent],
      },
      // The route with generic id must go last because otherwise
      // it will match the other ones as well
      {
        component: MultiRunJobComponent,
        path: 'job/multi-run/:id',
      },
      {
        component: EditProxyCheckJobComponent,
        path: 'job/proxy-check/edit',
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: EditProxyCheckJobComponent,
        path: 'job/proxy-check/create',
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: EditProxyCheckJobComponent,
        path: 'job/proxy-check/clone',
        canDeactivate: [canDeactivateFormComponent],
      },
      // The route with generic id must go last because otherwise
      // it will match the other ones as well
      {
        component: ProxyCheckJobComponent,
        path: 'job/proxy-check/:id',
      },
      {
        component: JobMonitorComponent,
        path: 'monitor',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: EditTriggeredActionComponent,
        path: 'monitor/triggered-action/edit',
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: EditTriggeredActionComponent,
        path: 'monitor/triggered-action/create',
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: EditTriggeredActionComponent,
        path: 'monitor/triggered-action/clone',
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: ProxiesComponent,
        path: 'proxies',
      },
      {
        component: WordlistsComponent,
        path: 'wordlists',
      },
      {
        component: HitsComponent,
        path: 'hits',
      },
      {
        component: ConfigsComponent,
        path: 'configs',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: ConfigMetadataComponent,
        path: 'config/metadata',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: ConfigReadmeComponent,
        path: 'config/readme',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: ConfigStackerComponent,
        path: 'config/stacker',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: ConfigLolicodeComponent,
        path: 'config/lolicode',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: ConfigLoliscriptComponent,
        path: 'config/loliscript',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: ConfigSettingsComponent,
        path: 'config/settings',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: ConfigCsharpComponent,
        path: 'config/csharp',
        resolve: { data: updateCSharpScript },
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: OBSettingsComponent,
        path: 'settings',
        canActivate: [() => inject(AdminGuard).canActivate()],
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: RlSettingsComponent,
        path: 'rl-settings',
        canActivate: [() => inject(AdminGuard).canActivate()],
        canDeactivate: [canDeactivateFormComponent],
      },
      {
        component: GuestsComponent,
        path: 'guests',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: PluginsComponent,
        path: 'plugins',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: SharingComponent,
        path: 'sharing',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
      {
        component: InfoComponent,
        path: 'info',
      },
      {
        component: ErrorDetailsComponent,
        path: 'error-details',
        canActivate: [() => inject(AdminGuard).canActivate()],
      },
    ],
  },
  {
    path: '**',
    component: NotFoundComponent,
    pathMatch: 'full',
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MainRoutingModule { }
