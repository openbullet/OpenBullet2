import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpErrorInterceptor } from 'src/interceptors/http-error.interceptor';
import { MenuComponent } from './components/menu/menu.component';
import { MainComponent } from './main.component';
import { MainRoutingModule } from './main-routing.module';
import { HomeComponent } from './components/home/home.component';



@NgModule({
  declarations: [
    MenuComponent,
    MainComponent,
    HomeComponent
  ],
  imports: [
    CommonModule,
    MainRoutingModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: HttpErrorInterceptor, multi: true }
  ]
})
export class MainModule { }
