import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable, catchError, tap, throwError } from "rxjs";
import { MessageService } from "primeng/api";
import { UserService } from "src/app/main/services/user.service";

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
    constructor(private router: Router,
        private messageService: MessageService,
        private userService: UserService) {

    }

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        // Inject the jwt if present
        const jwt = this.userService.getJwt();

        if (jwt !== null) {
            request = request.clone({
                setHeaders: {
                    Authorization: `Bearer ${jwt}`
                }
            });
        }

        return next.handle(request).pipe(
            tap((event: HttpEvent<any>) => {
                if (event instanceof HttpResponse) {
                    const headerValue = event.headers.get('X-Application-Warning');
                    if (headerValue) {
                        this.messageService.add({
                            severity: 'warn',
                            summary: 'Warning',
                            detail: headerValue
                        });
                    }
                }
            }),
            catchError(error => {
                if (error instanceof HttpErrorResponse) {
                    let showMessage = true;

                    // If unauthorized, redirect to login page
                    if (error.error.errorType === 'UNAUTHORIZED') {
                        this.router.navigate(['/login']);
                        showMessage = false;
                    }

                    let summary = 'Request Error';
                    let detail = error.error?.message ?? 'See details in the browser console';

                    // Status 0 or -1 means connection refused
                    if (error.status <= 0) {
                        summary = 'Network Error';
                        detail = 'Could not connect to the server';
                    }

                    if (showMessage) {
                        this.messageService.add({
                            severity: 'error',
                            summary,
                            detail
                        });
                    }
                }

                return throwError(() => error);
            })
        );
    }
}
