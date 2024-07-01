import {
  HttpErrorResponse,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpResponse,
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Observable, from, lastValueFrom } from 'rxjs';
import { UserService } from 'src/app/main/services/user.service';
import { ApiError } from '../models/api-error';

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
  private requireLoginErrorCodes: string[] = ['MISSING_AUTH_TOKEN', 'INVALID_AUTH_TOKEN', 'NOT_AUTHENTICATED', 'EXPIRED_GUEST_ACCOUNT', 'INVALID_GUEST_ACCOUNT'];

  constructor(
    private router: Router,
    private messageService: MessageService,
    private userService: UserService,
  ) { }

  // biome-ignore lint/suspicious/noExplicitAny: <explanation>
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<any> {
    return from(this.handle(request, next));
  }

  async handle(request: HttpRequest<unknown>, next: HttpHandler) {
    // Inject the jwt if present
    const jwt = this.userService.getJwt();
    let nextRequest = request;

    if (jwt !== null) {
      nextRequest = request.clone({
        setHeaders: {
          Authorization: `Bearer ${jwt}`,
        },
      });
    }

    try {
      const response = await lastValueFrom(next.handle(nextRequest));

      if (response instanceof HttpResponse) {
        const appWarning = response.headers.get('X-Application-Warning');
        if (appWarning) {
          this.messageService.add({
            severity: 'warn',
            summary: 'Warning',
            detail: appWarning,
          });
        }

        const newJwt = response.headers.get('X-New-Jwt');
        if (newJwt) {
          this.userService.saveJwt(newJwt);
        }
      }

      return response;
    } catch (error) {
      if (error instanceof HttpErrorResponse) {
        let showMessage = true;

        let apiError: ApiError = error.error;

        // If error is a blob with type application/json, try to parse it
        if (apiError instanceof Blob && error.headers.get('content-type')?.includes('application/json')) {
          try {
            apiError = await apiError.text().then((text) => JSON.parse(text));
          } catch (e) {
            console.error('Error parsing API error', e);
          }
        }

        // If unauthorized, clear any invalid jwt and redirect to login page
        if (this.requireLoginErrorCodes.includes(apiError.errorCode)) {
          this.userService.resetJwt();
          this.router.navigate(['/login']);
          showMessage = false;
        }

        let summary = 'Request Error';
        let detail = apiError?.message ?? 'See details in the browser console';

        // Status 0 or -1 means connection refused
        if (error.status <= 0) {
          summary = 'Network Error';
          detail = 'Could not connect to the server';
        }

        if (showMessage) {
          if (error.status >= 500 && error.status < 600) {
            summary = 'Server Error';
            detail = 'An error occurred on the server';

            this.messageService.add({
              severity: 'error',
              summary,
              detail,
              key: 'err500',
              life: 300 * 1000,
              data: error,
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary,
              detail,
            });
          }
        }
      }

      throw error;
    }
  }
}
