import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  // By default, go to main module's router
  {
    path: '**',
    loadChildren: () =>
      import('./main/main.module').then(
        m => m.MainModule
      ),
    pathMatch: 'full'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
