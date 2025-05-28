import { Routes } from '@angular/router';
import { UpdatePersonComponent } from './update-person/update-person.component';
import { PeopleTableComponent } from './people-table/people-table.component';

export const routes: Routes = [
    { path: '', component: PeopleTableComponent },
    { path: 'update/:name', component: UpdatePersonComponent }
];
