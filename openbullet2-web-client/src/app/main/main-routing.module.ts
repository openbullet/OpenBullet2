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
        component: OBSettingsComponent,
        path: 'settings'
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
