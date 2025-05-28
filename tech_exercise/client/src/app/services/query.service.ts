import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { PersonAstronaut } from '../data/personastronaut.data';
import { urlsData } from '../data/urls.data';

export interface BaseResponse {
  success: boolean;
  message: string;
  responseCode: number;
}

export interface PeopleResponse extends BaseResponse {
  people: PersonAstronaut[];
}

export interface PersonResponse {
  person: PersonAstronaut;
}

@Injectable({
  providedIn: 'root'
})
export class QueryService {
  readonly http = inject(HttpClient);
  
  fetchAllPeople() {
    return this.http.get<PeopleResponse>(urlsData.fetchPeople, {
      headers: { 'Content-Type': 'application/json' }
    }); 
  }

  fetchPersonByName(name: string) {
    return this.http.get<PersonResponse>(urlsData.fetchPersonByName(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  createPerson(name: string) {
    return this.http.post<PersonResponse>(urlsData.createPerson, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  updatePerson(data: { name: string; newName: string }) {
    return this.http.put<PersonResponse>(urlsData.updatePerson, JSON.stringify(data), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  fetchAstronautDutyByName(name: string) {
    return this.http.get<PersonResponse>(urlsData.fetchAstronautDutyByName(name), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  createAstronautDuty(data: any) {
    return this.http.post<PersonResponse>(urlsData.createAstronautDuty, JSON.stringify(data), {
      headers: { 'Content-Type': 'application/json' }
    });
  }
}
