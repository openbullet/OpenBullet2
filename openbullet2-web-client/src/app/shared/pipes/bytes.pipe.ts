import { Pipe, PipeTransform } from '@angular/core';
import { formatBytes } from '../utils/bytes';

@Pipe({
    name: 'bytes'
})

export class BytesPipe implements PipeTransform {
    transform(value: number, decimals: number = 2): string {
        return formatBytes(value, decimals);
    }
}
