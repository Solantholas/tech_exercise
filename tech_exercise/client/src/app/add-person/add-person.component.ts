import { Component, inject } from '@angular/core';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { PersonService } from '../services/person.service';
import { injectMutation, QueryClient } from '@tanstack/angular-query-experimental';

@Component({
  selector: 'app-add-person',
  standalone: true,
  imports: [CommonModule, FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './add-person.component.html',
  styleUrls: ['./add-person.component.scss']
})
export class AddPersonComponent {
  name: string = '';

  private readonly personService = inject(PersonService);
  private queryClient = inject(QueryClient);
  
  addPersonMutation = injectMutation(() => this.personService.addPerson());


  constructor(
    private dialogRef: MatDialogRef<AddPersonComponent>
  ) {}

  async submit() {
    if (this.name.trim()) {
      await this.addPersonMutation.mutateAsync(this.name.trim());
      this.dialogRef.close(this.name.trim());
    }
  }
}