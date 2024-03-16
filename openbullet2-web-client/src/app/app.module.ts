import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgxSpinnerModule } from 'ngx-spinner';
import { NgxSpinnerConfig } from 'ngx-spinner/lib/config';
import { NgChartsModule } from 'ng2-charts';
import { MessagesModule } from 'primeng/messages';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { HttpErrorInterceptor } from './shared/interceptors/http-error.interceptor';
import { MessageService } from 'primeng/api';
import { LoginComponent } from './login/login.component';
import { InputTextModule } from 'primeng/inputtext';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ForgotCredentialsComponent } from './login/forgot-credentials/forgot-credentials.component';
import { DialogModule } from 'primeng/dialog';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    ForgotCredentialsComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AppRoutingModule,
    FontAwesomeModule,
    FormsModule,
    DialogModule,
    ReactiveFormsModule,
    InputTextModule,
    HttpClientModule,
    NgxSpinnerModule.forRoot(<NgxSpinnerConfig>{
      type: 'cube-transition',
      bdColor: 'rgba(0, 0, 0, 0.8)',
      size: 'medium',
      color: '#fff'
    }),
    NgChartsModule,
    MessagesModule,
    ConfirmPopupModule,
    ConfirmDialogModule,
    ToastModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpErrorInterceptor,
      multi: true
    },
    MessageService
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
