import { Moment } from "moment";
import { TimeSpan } from "./timespan";

export function parseTimeSpan(input: string): TimeSpan {
    // We assume the format is [d:]HH:mm:ss
    const chunks = input.split(':');
    const days = chunks.length > 3 ? parseInt(chunks[0]) : 0;
    const hours = parseInt(chunks[1]);
    const minutes = parseInt(chunks[2]);
    const seconds = parseInt(chunks[3]);
    const milliSeconds = 0;
    return TimeSpan.fromTime(days, hours, minutes, seconds, milliSeconds);
}

export function addTimeSpan(momentDate: Moment, ts: TimeSpan): Moment {
    return momentDate
        .add(ts.days, 'days')
        .add(ts.hours, 'hours')
        .add(ts.minutes, 'minutes')
        .add(ts.seconds, 'seconds')
        .add(ts.milliseconds, 'milliseconds');
}
