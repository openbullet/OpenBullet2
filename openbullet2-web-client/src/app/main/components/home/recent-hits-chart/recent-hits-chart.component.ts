import { Component, Input, OnInit } from '@angular/core';
import { ChartConfiguration } from 'chart.js';
import { RecentHitsDto } from 'src/app/main/dtos/hit/recent-hits.dto';

@Component({
  selector: 'app-recent-hits-chart',
  templateUrl: './recent-hits-chart.component.html',
  styleUrls: ['./recent-hits-chart.component.scss'],
})
export class RecentHitsChartComponent implements OnInit {
  @Input() recentHits!: RecentHitsDto;

  barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [],
  };

  barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      x: {
        stacked: true,
      },
      y: {
        stacked: true,
        ticks: {
          maxTicksLimit: 5,
        },
      },
    },
  };

  mock: RecentHitsDto = {
    dates: ['2021-07-01', '2021-07-02', '2021-07-03', '2021-07-04', '2021-07-05', '2021-07-06', '2021-07-07'],
    hits: {
      config1: [1, 2, 3, 4, 5, 6, 7],
      config2: [5, 4, 3, 2, 1, 0, 0],
      config3: [1, 1, 1, 1, 1, 1, 1],
      config4: [2, 2, 2, 2, 2, 2, 2],
      config5: [3, 3, 3, 3, 3, 3, 3],
    },
  };

  ngOnInit(): void {
    // Sort the top 5 keys by the sum of their values
    const topK = Object.entries(this.recentHits.hits)
      .sort((a, b) => b[1].reduce((acc, val) => acc + val, 0) - a[1].reduce((acc, val) => acc + val, 0))
      .slice(0, 5);

    // Get chart colors from CSS variables
    const colors = Array.from({ length: 5 }, (_, i) => i + 1).map((i) =>
      getComputedStyle(document.documentElement).getPropertyValue(`--chart-series-${i}`),
    );

    this.barChartData = {
      labels: this.recentHits.dates.map((date) => new Date(date).toDateString()),
      datasets: topK.map(([configName, values], i) => {
        return {
          label: configName,
          data: values,
          barThickness: 40,
          backgroundColor: colors[i],
          borderColor: 'rgba(0, 0, 0, 0)',
          borderWidth: 2,
        };
      }),
    };
  }
}
