import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class VolatileSettingsService {
    public get configsDisplayMode() {
        // grid | table
        return window.localStorage.getItem('configsDisplayMode') ?? 'grid';
    }

    public set configsDisplayMode(mode: string) {
        window.localStorage.setItem('configsDisplayMode', mode);
    }
}
