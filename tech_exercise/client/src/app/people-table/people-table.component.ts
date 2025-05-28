import { Component, effect, inject } from '@angular/core';
import { PersonService } from '../services/person.service';
import { injectQuery, QueryClient } from '@tanstack/angular-query-experimental';
import { PersonAstronaut } from '../data/personastronaut.data';
import { PersonDetailsComponent } from '../person-details/person-details.component';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { CamelCasePipe } from '../pipes/camel-case.pipe';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-people-table',
  imports: [CommonModule, MatTableModule, MatButtonModule, MatProgressSpinnerModule, MatCardModule, MatDividerModule, MatTooltipModule, PersonDetailsComponent, CamelCasePipe],
  templateUrl: './people-table.component.html',
  styleUrls: ['./people-table.component.scss', '../app.component.scss'],
})
export class PeopleTableComponent {
  private readonly personService = inject(PersonService);
  readonly queryClient = inject(QueryClient);

  peopleQuery = injectQuery(() => this.personService.fetchPeople());

  private dialog = inject(MatDialog);
  private router = inject(Router);
  
  displayedColumns: string[] = ['name', 'currentRank', 'currentDutyTitle', 'careerStartDate', 'careerEndDate', 'actions'];
  dataSource = new MatTableDataSource<PersonAstronaut>();

  openDetails(person: PersonAstronaut) {
    this.dialog.open(PersonDetailsComponent, {
      data: person
    });
  }

  goToUpdate(person: PersonAstronaut) {
    this.router.navigate(['/update', person.name]);
  }
}

