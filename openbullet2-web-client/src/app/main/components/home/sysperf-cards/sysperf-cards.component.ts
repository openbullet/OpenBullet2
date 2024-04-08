import { formatNumber } from '@angular/common';
import { Component, OnDestroy, OnInit, QueryList, ViewChildren } from '@angular/core';
import {
  IconDefinition,
  faCaretDown,
  faCaretUp,
  faCircleArrowDown,
  faCircleArrowUp,
  faCircleMinus,
  faTrashCan,
} from '@fortawesome/free-solid-svg-icons';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { MessageService } from 'primeng/api';
import { Subscription } from 'rxjs';
import { PerformanceInfoDto } from 'src/app/main/dtos/info/performance-info.dto';
import { DebugService } from 'src/app/main/services/debug.service';
import { SysPerfHubService } from 'src/app/main/services/sysperf.hub.service';
import { UserService } from 'src/app/main/services/user.service';
import { formatBytes } from 'src/app/shared/utils/bytes';

@Component({
  selector: 'app-sysperf-cards',
  templateUrl: './sysperf-cards.component.html',
  styleUrls: ['./sysperf-cards.component.scss'],
})
export class SysperfCardsComponent implements OnInit, OnDestroy {
  CHART_DATA_POINTS = 60;
  faCircleArrowUp = faCircleArrowUp;
  faCircleArrowDown = faCircleArrowDown;
  faCircleMinus = faCircleMinus;
  faCaretDown = faCaretDown;
  faCaretUp = faCaretUp;
  faTrashCan = faTrashCan;

  // CPU
  cpuChipValue = 0;
  cpuChipClass = 'perf-flat';
  cpuChipIcon = faCircleMinus;
  cpuValue = 0;

  // MEMORY
  memoryChipValue = 0;
  memoryChipClass = 'perf-flat';
  memoryChipIcon = faCircleMinus;
  memoryValue = 0;

  // NETWORK
  networkDownloadValue = 0;
  networkUploadValue = 0;

  cpuLineChartData: ChartConfiguration['data'] = {
    datasets: [
      {
        data: [],
        label: '',
        backgroundColor: 'rgba(0, 0, 0, 0)',
        borderColor: 'rgba(148,159,177,1)',
        fill: false,
        tension: 0,
      },
    ],
    labels: Array(this.CHART_DATA_POINTS).fill(''),
  };

  memoryLineChartData: ChartConfiguration['data'] = {
    datasets: [
      {
        data: [],
        label: '',
        backgroundColor: 'rgba(0, 0, 0, 0)',
        borderColor: 'rgba(148,159,177,1)',
        fill: false,
        tension: 0,
      },
    ],
    labels: Array(this.CHART_DATA_POINTS).fill(''),
  };

  networkLineChartData: ChartConfiguration['data'] = {
    datasets: [
      // Download
      {
        data: [],
        label: '',
        backgroundColor: 'rgba(0, 0, 0, 0)',
        borderColor: 'rgba(50,90,245,1)',
        fill: false,
        tension: 0,
      },
      // Upload
      {
        data: [],
        label: '',
        backgroundColor: 'rgba(0, 0, 0, 0)',
        borderColor: 'rgba(100,100,100,1)',
        fill: false,
        tension: 0,
      },
    ],
    labels: Array(this.CHART_DATA_POINTS).fill(''),
  };

  lineChartOptions: ChartConfiguration['options'] = <ChartOptions>{
    events: [], // Remove this to get back the tooltips on hover
    elements: {
      line: {
        tension: 0.5,
      },
      point: {
        radius: 0,
      },
    },
    scales: {
      x: {
        grid: {
          display: false,
          color: 'rgba(0, 0, 0, 0)',
          drawBorder: false,
        },
        angleLines: {
          display: false,
        },
        ticks: {
          display: false,
        },
      },
      y: {
        grid: {
          display: false,
          color: 'rgba(0, 0, 0, 0)',
          drawBorder: false,
        },
        angleLines: {
          display: false,
        },
        beginAtZero: true,
        ticks: {
          display: true,
          maxTicksLimit: 2,
          callback: function (value, index, values) {
            if (values.length === 0) return value;
            if (typeof value === 'string') return;

            const chartId = this.chart.canvas.id;

            if (chartId === 'cpuChart') {
              return `${formatNumber(value, 'en-US', '1.0-0')}%`;
            }

            if (chartId === 'networkChart') {
              return `${formatBytes(value, 0)}/s`;
            }

            if (chartId === 'memoryChart') {
              return formatBytes(value, 0);
            }

            return value;
          },
        },
      },
    },
    animation: {
      duration: 0,
    },
    plugins: {
      legend: { display: false },
    },
  };

  metricsSubscription: Subscription | null = null;
  isAdmin: boolean;

  @ViewChildren(BaseChartDirective) charts?: QueryList<BaseChartDirective>;

  constructor(
    private sysPerfHubService: SysPerfHubService,
    private debugService: DebugService,
    private messageService: MessageService,
    private userService: UserService,
  ) {
    this.isAdmin = this.userService.isAdmin();
  }

  ngOnInit(): void {
    // Mocked metrics, to use when debugging
    /*
    setInterval(() => {
      this.onNewMetrics(getMockedSysPerfMetrics())
    }, 1000);
    */

    this.sysPerfHubService.createHubConnection();
    this.metricsSubscription = this.sysPerfHubService.metrics$.subscribe((metrics) => {
      if (metrics !== null) {
        this.onNewMetrics(metrics);
      }
    });
  }

  ngOnDestroy(): void {
    this.sysPerfHubService.stopHubConnection();

    this.metricsSubscription?.unsubscribe();
  }

  garbageCollect() {
    this.debugService.garbageCollect().subscribe(() => {
      this.messageService.add({
        severity: 'info',
        summary: 'Requested',
        detail: 'Garbage Collection is on the way!',
      });
    });
  }

  onNewMetrics(perf: PerformanceInfoDto) {
    const cpuChart = this.charts?.get(0);
    const networkChart = this.charts?.get(1);
    const memoryChart = this.charts?.get(2);

    this.cpuValue = perf.cpuUsage;
    this.memoryValue = perf.memoryUsage;
    this.networkDownloadValue = perf.networkDownload;
    this.networkUploadValue = perf.networkUpload;

    // CPU
    const cpuData = this.cpuLineChartData.datasets[0].data as number[];

    if (cpuData.length >= this.CHART_DATA_POINTS) {
      cpuData.shift();
    }
    cpuData.push(perf.cpuUsage);

    const cpuChip = this.getChipInfo(cpuData);
    this.cpuChipClass = cpuChip[0];
    this.cpuChipIcon = cpuChip[1];
    this.cpuChipValue = cpuChip[2];

    cpuChart?.update();

    // MEMORY
    const memoryData = this.memoryLineChartData.datasets[0].data as number[];

    if (memoryData.length >= this.CHART_DATA_POINTS) {
      memoryData.shift();
    }
    memoryData.push(perf.memoryUsage);

    const memoryChip = this.getChipInfo(memoryData);
    this.memoryChipClass = memoryChip[0];
    this.memoryChipIcon = memoryChip[1];
    this.memoryChipValue = memoryChip[2];

    memoryChart?.update();

    // NETWORK
    const networkDownloadData = this.networkLineChartData.datasets[0].data as number[];
    const networkUploadData = this.networkLineChartData.datasets[1].data as number[];

    if (networkDownloadData.length >= this.CHART_DATA_POINTS) {
      networkDownloadData.shift();
    }
    networkDownloadData.push(perf.networkDownload);

    if (networkUploadData.length >= this.CHART_DATA_POINTS) {
      networkUploadData.shift();
    }
    networkUploadData.push(perf.networkUpload);

    networkChart?.update();
  }

  getChipInfo(data: number[]): [string, IconDefinition, number] {
    // If we don't have enough data, return flat
    if (data.length < 2) {
      return ['perf-flat', faCircleMinus, 0];
    }

    // TODO: Use the first non-zero here!
    const first = data[0];
    const last = data[data.length - 1];

    // If the first is 0, we cannot divide by 0
    if (first === 0) {
      if (last === 0) {
        return ['perf-flat', faCircleMinus, 0];
      }

      if (last > 0) {
        return ['perf-bad', faCircleArrowUp, 0];
      }

      return ['perf-good', faCircleArrowDown, 0];
    }

    const percIncrement = (last - first) / first;

    if (percIncrement === 0) {
      return ['perf-flat', faCircleMinus, 0];
    }

    if (percIncrement > 0) {
      return ['perf-bad', faCircleArrowUp, percIncrement * 100];
    }

    return ['perf-good', faCircleArrowDown, percIncrement * 100];
  }
}
