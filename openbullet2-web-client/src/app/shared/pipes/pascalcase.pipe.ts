import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'pascalcase',
})
export class PascalCasePipe implements PipeTransform {
  transform(value: string, spaces = true): string {
    let newValue = value;
    if (spaces) {
      newValue = newValue.replace(/([A-Z])/g, ' $1');
    }

    return newValue.replace(/^./, (str) => str.toUpperCase());
  }
}
