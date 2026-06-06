import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { ConfigMode } from '../dtos/config/config-info.dto';
import { ConfigService } from '../services/config.service';

// biome-ignore lint/suspicious/noExplicitAny: any
export const updateCSharpScript: ResolveFn<any> = async (route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
  const configService = inject(ConfigService);
  const config = configService.selectedConfig;

  // If the config is null, load the page, there will be
  // a warning in the page itself notifying the user
  if (config === null) {
    return;
  }

  // If the config's mode is loliCode, we need to convert the
  // loliCode to C# first
  if (config.mode === ConfigMode.LoliCode || config.mode === ConfigMode.Stack) {
    const loliCode = config.loliCodeScript ?? '';
    const startupLoliCode = config.startupLoliCodeScript ?? '';

    if (loliCode.trim() === '') {
      config.cSharpScript = '';
    } else {
      const dto = await lastValueFrom(configService.convertLoliCodeToCSharp(config.settings, loliCode));
      config.cSharpScript = dto.cSharpScript;
    }

    if (startupLoliCode.trim() !== '') {
      const dto = await lastValueFrom(configService.convertLoliCodeToCSharp(config.settings, startupLoliCode));
      config.startupCSharpScript = dto.cSharpScript;
    } else {
      config.startupCSharpScript = '';
    }
  }
};
