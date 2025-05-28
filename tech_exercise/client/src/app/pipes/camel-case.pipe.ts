import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'camelCase', standalone: true })
export class CamelCasePipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '';
    return value
      .toLowerCase()
      .replace(/(?:^|\s|_)\S/g, match => match.toUpperCase())
      .replace(/_/g, ' ');
  }
}