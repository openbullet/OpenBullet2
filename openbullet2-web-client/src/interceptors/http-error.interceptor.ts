import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import { ToastrService } from "ngx-toastr";
import { Observable, catchError, throwError } from "rxjs";

@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
    constructor(private router: Router, private toastr: ToastrService) {
        
    }

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        return next.handle(request).pipe(
            catchError(error => {
                if (error) {
                    this.toastr.error('Error');
                    console.log(error);
                }

                return throwError(() => error);
            })
        );
    }
}
