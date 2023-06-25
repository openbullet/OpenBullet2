import { inject } from "@angular/core";
import { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from "@angular/router";
import { ConfigService } from "../services/config.service";
import { lastValueFrom } from "rxjs";
import { MessageService } from "primeng/api";
import { HttpErrorResponse } from "@angular/common/http";

export const updateCSharpScript: ResolveFn<any> =
    async (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
        const configService = inject(ConfigService);
        const config = configService.selectedConfig;

        // If the config is null, load the page, there will be
        // a warning in the page itself notifying the user
        if (config === null) {
            return;
        }

        // If the config's mode is loliCode, we need to convert the
        // loliCode to C# first
        if (config.mode === 'loliCode') {
            const dto = await lastValueFrom(configService.convertLoliCodeToCSharp(
                config.settings, config.loliCodeScript));

            config.cSharpScript = dto.cSharpScript;
        }
    };
    