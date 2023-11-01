import { Pipe, PipeTransform } from '@angular/core';
import { parseTimeSpan } from '../utils/dates';

@Pipe({
 name: 'timespan'
})

export class TimeSpanPipe implements PipeTransform {
    transform(value: string): string {
        const timeSpan = parseTimeSpan(value);
        let output = '';

        // Return [d day[s]]HH:mm:ss

        if (timeSpan.days > 0) {
            output += timeSpan.days + timeSpan.days === 1 ? ' day ' : ' days ';
        }

        output += timeSpan.hours.toString().padStart(2, '0') + ':';
        output += timeSpan.minutes.toString().padStart(2, '0') + ':';
        output += timeSpan.seconds.toString().padStart(2, '0');

        return output;
    }
}
