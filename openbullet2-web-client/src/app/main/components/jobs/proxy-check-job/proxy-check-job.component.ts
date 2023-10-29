import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PCJNewResultMessage } from 'src/app/main/dtos/job/messages/proxy-check/new-result.dto';
import { getMockedProxyCheckJobNewResultMessage } from 'src/app/main/mock/messages.mock';
import { ProxyCheckJobHubService } from 'src/app/main/services/proxy-check-job.hub.service';

@Component({
  selector: 'app-proxy-check-job',
  templateUrl: './proxy-check-job.component.html',
  styleUrls: ['./proxy-check-job.component.scss']
})
export class ProxyCheckJobComponent implements OnInit, OnDestroy {
  jobId: number | null = null;

  constructor(
    activatedRoute: ActivatedRoute,
    private router: Router,
    private proxyCheckJobHubService: ProxyCheckJobHubService
  ) { 
    activatedRoute.url.subscribe(url => {
      this.jobId = parseInt(url[2].path);
    });
  }

  ngOnInit(): void {
    if (this.jobId === null) {
      this.router.navigate(['/jobs']);
      return;
    }

    // Mocked results, to use when debugging
    /*
    setInterval(() => {
      this.onNewResult(getMockedProxyCheckJobNewResultMessage())
    }, 1000);
    */

    this.proxyCheckJobHubService.createHubConnection(this.jobId);
    this.proxyCheckJobHubService.result$.subscribe(result => {
      if (result !== null) {
        this.onNewResult(result);
      }
    });
  }

  ngOnDestroy(): void {
    this.proxyCheckJobHubService.stopHubConnection();
  }

  onNewResult(result: PCJNewResultMessage) {
    console.log(result);
  }
}
