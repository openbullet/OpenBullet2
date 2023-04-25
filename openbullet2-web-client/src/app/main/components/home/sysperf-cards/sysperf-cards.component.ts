import { Component, OnInit, QueryList, ViewChildren } from '@angular/core';
import { 
  IconDefinition, 
  faCircleArrowDown, 
  faCircleArrowUp, 
  faCircleMinus ,
  faCaretDown,
  faCaretUp
} from '@fortawesome/free-solid-svg-icons';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { PerformanceInfoDto } from 'src/app/main/dtos/info/performance-info.dto';

@Component({
  selector: 'app-sysperf-cards',
  templateUrl: './sysperf-cards.component.html',
  styleUrls: ['./sysperf-cards.component.scss']
})
export class SysperfCardsComponent implements OnInit {
  CHART_DATA_POINTS = 60;
  faCircleArrowUp = faCircleArrowUp;
  faCircleArrowDown = faCircleArrowDown;
  faCircleMinus = faCircleMinus;
  faCaretDown = faCaretDown;
  faCaretUp = faCaretUp;

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
        tension: 0
      }
    ],
    labels: Array(this.CHART_DATA_POINTS).fill('')
  };

  memoryLineChartData: ChartConfiguration['data'] = {
    datasets: [
      {
        data: [],
        label: '',
        backgroundColor: 'rgba(0, 0, 0, 0)',
        borderColor: 'rgba(148,159,177,1)',
        fill: false,
        tension: 0
      }
    ],
    labels: Array(this.CHART_DATA_POINTS).fill('')
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
        tension: 0
      },
      // Upload
      {
        data: [],
        label: '',
        backgroundColor: 'rgba(0, 0, 0, 0)',
        borderColor: 'rgba(100,100,100,1)',
        fill: false,
        tension: 0
      }
    ],
    labels: Array(this.CHART_DATA_POINTS).fill('')
  };

  lineChartOptions: ChartConfiguration['options'] = <ChartOptions>{
    events: [], // Remove this to get back the tooltips on hover
    elements: {
      line: {
        tension: 0.5
      },
      point: {
        radius: 0
      }
    },
    scales: {
      x: {
        grid: {
          display: false,
          color: 'rgba(0, 0, 0, 0)',
          drawBorder: false
        },
        angleLines: {
          display: false
        },
        ticks: {
          display: false
        }
      },
      y: {
        grid: {
          display: false,
          color: 'rgba(0, 0, 0, 0)',
          drawBorder: false
        },
        angleLines: {
          display: false
        },
        ticks: {
          display: false
        }
      }
    },
    animation: {
      duration: 0
    },
    plugins: {
      legend: { display: false }
    }
  };

  @ViewChildren(BaseChartDirective) charts?: QueryList<BaseChartDirective>;

  ngOnInit(): void {
    setInterval(() => {
      this.onNewMetrics({
        memoryUsage: Math.floor(Math.random() * 20),
        cpuUsage: Math.random() * 100,
        networkDownload: Math.floor(Math.random() * 20),
        networkUpload: Math.floor(Math.random() * 20)
      })
    }, 1000);
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
      } else if (last > 0) {
        return ['perf-up', faCircleArrowUp, 0];
      } else {
        return ['perf-down', faCircleArrowDown, 0];
      }
    }

    const percIncrement = (last - first) / first;

    if (percIncrement === 0) {
      return ['perf-flat', faCircleMinus, 0];
    }
    else if (percIncrement > 0) {
      return ['perf-up', faCircleArrowUp, percIncrement * 100];
    }
    else {
      return ['perf-down', faCircleArrowDown, percIncrement * 100];
    }
  }
}
