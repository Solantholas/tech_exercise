import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { PersonAstronaut } from '../data/personastronaut.data';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { CamelCasePipe } from '../pipes/camel-case.pipe';

@Component({
  selector: 'app-person-details',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatListModule, CamelCasePipe],
  templateUrl: './person-details.component.html',
})

export class PersonDetailsComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public data: PersonAstronaut) {}
}