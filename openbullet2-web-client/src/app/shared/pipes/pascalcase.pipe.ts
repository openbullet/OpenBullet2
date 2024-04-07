import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'pascalcase',
})
export class PascalCasePipe implements PipeTransform {
  transform(value: string, spaces = true): string {
    if (spaces) {
      value = value.replace(/([A-Z])/g, ' $1');
    }

    return value.replace(/^./, (str) => str.toUpperCase());
  }
}
