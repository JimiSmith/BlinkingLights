import {
  Component,
  Input,
  OnDestroy,
  ViewChild,
  ElementRef,
  AfterViewInit
} from '@angular/core';
import { SmoothieChart, TimeSeries } from 'smoothie';
import { SignalRService } from '../signal-r.service';
import { Subscription } from 'rxjs';
import * as moment from 'moment';

@Component({
  selector: 'app-realtime-chart',
  templateUrl: './realtime-chart.component.html',
  styleUrls: ['./realtime-chart.component.css']
})
export class RealtimeChartComponent implements AfterViewInit, OnDestroy {
  @ViewChild('chart') chartElementRef: ElementRef;
  @Input('SubscriptionName') SubscriptionName: string;

  series: TimeSeries;
  chart: SmoothieChart;
  subscription: Subscription;

  constructor(
    private hostElementRef: ElementRef,
    private signalRService: SignalRService
  ) {}

  ngAfterViewInit() {
    const parent = this.hostElementRef.nativeElement.parentNode as HTMLElement;
    const canvas = this.chartElementRef.nativeElement as HTMLCanvasElement;
    canvas.width = parent.clientWidth;
    canvas.height = parent.clientHeight;
    canvas.style['width'] = '100%';
    canvas.style['height'] = '100%';
    this.subscription = this.signalRService
      .on(this.SubscriptionName)
      .subscribe(data => {
        this.series.append(
          moment(data.time)
            .toDate()
            .getTime(),
          data.value
        );
      });
    this.chart = new SmoothieChart({
      tooltip: true,
      timestampFormatter: SmoothieChart.timeFormatter,
      maxValue: 80,
      minValue: -20,
      grid: { millisPerLine: 2000 }
    });
    this.series = new TimeSeries();

    this.chart.addTimeSeries(this.series, {
      lineWidth: 2,
      strokeStyle: '#00ff00'
    });
    this.chart.streamTo(canvas, 10000);
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }
}
