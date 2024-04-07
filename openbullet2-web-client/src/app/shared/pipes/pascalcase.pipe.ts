import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'pascalcase',
})
export class PascalCasePipe implements PipeTransform {
  transform(value: string, spaces: boolean = true): string {
    if (spaces) {
      value = value.replace(/([A-Z])/g, ' $1');
    }

    return value.replace(/^./, function (str) {
      return str.toUpperCase();
    });
  }
}
