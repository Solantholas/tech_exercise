import { Component, inject, effect, signal, input, Input } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PersonService } from '../services/person.service';
import { injectMutation, injectQuery, QueryClient, QueryObserver } from '@tanstack/angular-query-experimental';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PersonAstronaut } from '../data/personastronaut.data';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { AstronautRank } from '../data/astronaut-rank.enum';
import { AstronautDutyService } from '../services/astronaut-duty.service';
import { CamelCasePipe } from '../pipes/camel-case.pipe';
import { AstronautDutyTitle } from '../data/astronaut-duty-title.enum';

@Component({
  selector: 'app-update-person',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatSelectModule,
    CamelCasePipe
  ],
  templateUrl: './update-person.component.html',
  styleUrls: ['./update-person.component.scss', '../app.component.scss']
})

export class UpdatePersonComponent {
  @Input() name: string = '';
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly personService = inject(PersonService);
  private readonly astronautDutyService = inject(AstronautDutyService);
  private readonly queryClient = inject(QueryClient);
  private readonly fb = inject(FormBuilder);

  personQuery = injectQuery(() =>
    this.personService.fetchPersonByName(this.name)
  );

  personMutation = injectMutation(() => this.personService.updatePerson());
  astronautDutyMutation = injectMutation(() => this.astronautDutyService.createAstronautDuties());

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required]],
    currentRank: [''],
    currentDutyTitle: [''],
    careerStartDate: ['']
  });

  readonly astronautRanks = Object.values(AstronautRank);
  readonly astronautDutyTitles = Object.values(AstronautDutyTitle);

  constructor() {
    this.form.patchValue({ name: this.name });

    effect(() => {
      const person = this.personQuery.data()?.person;

      if (person) {
        this.form.patchValue({
          name: person.name ?? '',
          currentRank: person.currentRank ?? '',
          currentDutyTitle: person.currentDutyTitle ?? '',
          careerStartDate: person.careerStartDate ?? ''
        });
      }
    });
  }

  goBack() {
    this.router.navigate(['/']);
  }

  async handleSubmit(event: SubmitEvent) {
    event.preventDefault();
    event.stopPropagation();
    
    if (this.form.valid) {
      // if the name has not changed, we can skip the update
      const { person }: any = this.personQuery?.data();

      await this.updatePerson(person);
      await this.updateAstronautDuties(person);

      this.router.navigate(['/']);
    }
  }

  async updatePerson(person: PersonAstronaut) {
    if (this.form.value.name?.trim() === this.name) {
      return;
    }

     await this.personMutation.mutateAsync({
        name: this.personQuery?.data()?.person?.name ?? '',
        newName: this.form.value.name?.trim() ?? ''
      });
  }

  async updateAstronautDuties(person: PersonAstronaut) {
    const formValue = this.form.value;

    if (
      formValue.currentRank === person.currentRank &&
      formValue.currentDutyTitle === person.currentDutyTitle &&
      formValue.careerStartDate === person.careerStartDate
    ) {
      return;
    }

    const startDate = this.form.value.careerStartDate;
    const isoStartDate = startDate ? new Date(startDate).toISOString() : '';

    // ensure form rank is uppercase
    if (formValue.currentRank) {
      formValue.currentRank = formValue.currentRank.toUpperCase();
    }
    
    await this.astronautDutyMutation.mutateAsync({
      name: this.form.value.name?.trim() ?? '',
      rank: (this.form.value.currentRank ?? '').toUpperCase(),
      dutyTitle: this.form.value.currentDutyTitle ?? '',
      dutyStartDate: isoStartDate
    });
  }
}
