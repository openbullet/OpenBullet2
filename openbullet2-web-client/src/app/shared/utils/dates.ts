import { Moment } from 'moment';
import { TimeSpan } from './timespan';

export function parseTimeSpan(input: string): TimeSpan {
  // We assume the format is [-][d.]HH:mm:ss[.millis] e.g. 5.04:01:02.0030010
  // means 5 days, 4 hours, 1 minute, 2 seconds, 3 milliseconds and 1 microsecond

  if (input.startsWith('-')) {
    input = input.substring(1);
  }

  let days = 0;
  let millis = 0;
  const bigChunks = input.split('.');

  if (bigChunks.length > 2) {
    days = parseInt(bigChunks[0]);
    input = bigChunks[1];
  }

  if (bigChunks.length === 3) {
    millis = parseInt(bigChunks[2].substring(0, 3));
  }

  const chunks = input.split(':');
  const hours = parseInt(chunks[0]);
  const minutes = parseInt(chunks[1]);
  const seconds = parseInt(chunks[2]);

  return TimeSpan.fromTime(days, hours, minutes, seconds, millis);
}

export function addTimeSpan(momentDate: Moment, ts: TimeSpan): Moment {
  return momentDate
    .add(ts.days, 'days')
    .add(ts.hours, 'hours')
    .add(ts.minutes, 'minutes')
    .add(ts.seconds, 'seconds')
    .add(ts.milliseconds, 'milliseconds');
}
