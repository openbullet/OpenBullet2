import { CanDeactivateFn } from '@angular/router';
import { Observable } from 'rxjs';

export interface DeactivatableComponent {
  canDeactivate: () => boolean | Observable<boolean>;
}

export const canDeactivateFormComponent: CanDeactivateFn<DeactivatableComponent> = (
  component: DeactivatableComponent,
) => {
  if (component.canDeactivate) {
    return component.canDeactivate();
  }
  return true;
};
