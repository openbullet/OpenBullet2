import { Pipe, PipeTransform } from '@angular/core';
import { parseTimeSpan } from '../utils/dates';
import { TimeSpan } from '../utils/timespan';

@Pipe({
  name: 'timespan',
})
export class TimeSpanPipe implements PipeTransform {
  transform(value: string | TimeSpan | null): string {
    if (value === null) {
      return 'unknown time';
    }

    const timeSpan = typeof value === 'string' ? parseTimeSpan(value) : value;
    let output = '';

    // Return [d day[s]]HH:mm:ss

    if (timeSpan.days > 0) {
      output += timeSpan.days + (timeSpan.days === 1 ? ' day ' : ' days ');
    }

    output += `${timeSpan.hours.toString().padStart(2, '0')}:`;
    output += `${timeSpan.minutes.toString().padStart(2, '0')}:`;
    output += timeSpan.seconds.toString().padStart(2, '0');

    return output;
  }
}
