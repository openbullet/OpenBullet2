import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpErrorInterceptor } from 'src/interceptors/http-error.interceptor';
import { MenuComponent } from './components/menu/menu.component';
import { MainComponent } from './main.component';
import { MainRoutingModule } from './main-routing.module';
import { HomeComponent } from './components/home/home.component';
import { MenuItemComponent } from './components/menu/menu-item/menu-item.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { ChipModule } from 'primeng/chip';
import { MessagesModule } from 'primeng/messages';
import { AvatarModule } from 'primeng/avatar';
import { CalendarModule } from 'primeng/calendar';
import { AccordionModule } from 'primeng/accordion';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { TableModule } from 'primeng/table';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MarkdownModule } from 'ngx-markdown';
import { SharedModule } from '../shared/shared.module';
import { NgxSpinnerModule } from 'ngx-spinner';
import { NgChartsModule } from 'ng2-charts';
import { SysperfCardsComponent } from './components/home/sysperf-cards/sysperf-cards.component';
import { InfoComponent } from './components/info/info.component';
import { ContributorComponent } from './components/info/contributor/contributor.component';
import { GuestsComponent } from './components/guests/guests.component';
import { CreateGuestComponent } from './components/guests/create-guest/create-guest.component';
import { UpdateGuestInfoComponent } from './components/guests/update-guest-info/update-guest-info.component';
import { UpdateGuestPasswordComponent } from './components/guests/update-guest-password/update-guest-password.component';

@NgModule({
  declarations: [
    MenuComponent,
    MainComponent,
    HomeComponent,
    MenuItemComponent,
    NotFoundComponent,
    SysperfCardsComponent,
    InfoComponent,
    ContributorComponent,
    GuestsComponent,
    CreateGuestComponent,
    UpdateGuestInfoComponent,
    UpdateGuestPasswordComponent
  ],
  imports: [
    CommonModule,
    MainRoutingModule,
    FontAwesomeModule,
    FormsModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    AvatarModule,
    TooltipModule,
    MessagesModule,
    AccordionModule,
    CalendarModule,
    ToastModule,
    ConfirmPopupModule,
    ConfirmDialogModule,
    DialogModule,
    TableModule,
    ChipModule,
    NgChartsModule,
    MarkdownModule.forRoot(),
    SharedModule,
    NgxSpinnerModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: HttpErrorInterceptor, multi: true }
  ]
})
export class MainModule { }
