import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import {HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import { Observable, catchError, throwError } from "rxjs";
import { MessageService } from "primeng/api";

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
    constructor(private router: Router, private messageService: MessageService) {
        
    }

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        return next.handle(request).pipe(
            catchError(error => {
                if (error instanceof HttpErrorResponse) {
                    let summary = 'Request Error';
                    let detail = error.error?.message ?? 'See details in the browser console';

                    // Status 0 or -1 means connection refused
                    if (error.status <= 0) {
                        summary = 'Network Error';
                        detail = 'Could not connect to the server';
                    }

                    this.messageService.add({
                        severity: 'error',
                        summary,
                        detail 
                    });
                }

                return throwError(() => error);
            })
        );
    }
}
