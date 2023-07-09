import { Injectable } from "@angular/core";
import { UserInfo } from "../models/user-info";
import { JwtHelperService } from '@auth0/angular-jwt';
import { HttpClient } from "@angular/common/http";
import { UserLoginDto } from "../dtos/user/user-login.dto";
import { LoggedInUserDto } from "../dtos/user/logged-in-user.dto";
import { getBaseUrl } from "src/app/shared/utils/host";

@Injectable({
    providedIn: 'root'
})
export class UserService {
    constructor(
        private http: HttpClient
    ) { }

    resetJwt() {
        window.localStorage.removeItem('jwt');
    }

    saveJwt(jwt: string) {
        window.localStorage.setItem('jwt', jwt);
    }

    getJwt(): string | null {
        return window.localStorage.getItem('jwt');
    }

    loadUserInfo(): UserInfo {
        const jwt = this.getJwt();

        // By default, if no authentication is provided, we assume
        // the user is an admin and that authentication is not
        // enforced. If there is any trouble, the backend will
        // block the requests anyways and make the user log in again.
        const defaultUserInfo: UserInfo = {
            username: 'admin',
            role: 'admin'
        };

        if (jwt === null) {
            return defaultUserInfo;
        }

        const helper = new JwtHelperService();

        if (helper.isTokenExpired(jwt)) {
            return defaultUserInfo;
        }

        const decodedToken = helper.decodeToken(jwt);
        const name = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
        const role = decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

        return {
            username: name,
            role: role.toLowerCase()
        };
    }

    login(user: UserLoginDto) {
        return this.http.post<LoggedInUserDto>(
            getBaseUrl() + '/user/login', user
        );
    }
}
