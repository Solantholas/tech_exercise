import { Component } from '@angular/core';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { AddPersonComponent } from '../add-person/add-person.component';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-drawer',
  standalone: true,
  imports: [CommonModule, MatSidenavModule, MatButtonModule],
  templateUrl: './drawer.component.html',
  styleUrls: ['./drawer.component.scss']
})
export class DrawerComponent {
  opened = false;

  constructor(private dialog: MatDialog, private router: Router) {}

  openAddPerson() {
    this.dialog.open(AddPersonComponent);
  }

  navigateToPeopleTable() {
    this.router.navigate(['/']);
  }
}