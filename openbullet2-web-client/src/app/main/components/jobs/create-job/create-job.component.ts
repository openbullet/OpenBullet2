import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-create-job',
  templateUrl: './create-job.component.html',
  styleUrls: ['./create-job.component.scss'],
})
export class CreateJobComponent {
  constructor(private router: Router) {}

  createMultiRunJob() {
    this.router.navigate(['/job/multi-run/create']);
  }

  createProxyCheckJob() {
    this.router.navigate(['/job/proxy-check/create']);
  }
}
