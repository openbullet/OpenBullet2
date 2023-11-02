import { Component, HostListener } from '@angular/core';
import { ConfirmationService } from 'primeng/api';
import { Observable } from 'rxjs';
import { DeactivatableComponent } from 'src/app/shared/guards/can-deactivate-form.guard';

@Component({
  selector: 'app-edit-multi-run-job',
  templateUrl: './edit-multi-run-job.component.html',
  styleUrls: ['./edit-multi-run-job.component.scss']
})
export class EditMultiRunJobComponent implements DeactivatableComponent {
  @HostListener('window:beforeunload') confirmLeavingWithoutSaving(): boolean {
    return !this.touched;
  }

  touched: boolean = false;

  constructor(private confirmationService: ConfirmationService) { }

  canDeactivate() {
    if (!this.touched) {
      return true;
    }

    // Ask for confirmation and return the observable
    return new Observable<boolean>(observer => {
      this.confirmationService.confirm({
        message: `You have unsaved changes. Are you sure that you want to leave?`,
        header: 'Confirmation',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          observer.next(true);
          observer.complete();
        },
        reject: () => {
          observer.next(false);
          observer.complete();
        }
      });
    });
  }
}
