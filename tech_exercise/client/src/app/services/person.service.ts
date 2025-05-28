import { Injectable, inject } from '@angular/core';
import { urlsData } from '../data/urls.data';
import { HttpClient } from '@angular/common/http';
import { lastValueFrom, Observable } from 'rxjs';
import { mutationOptions, QueryClient, queryOptions } from '@tanstack/angular-query-experimental';
import { QueryService } from './query.service';
import { QueryKey } from '../data/query-key.enum';
import { PersonAstronaut } from '../data/personastronaut.data';

export interface Response {
  people: PersonAstronaut[];
  success: boolean;
  message: string;
  responseCode: number;
}

@Injectable({ providedIn: 'root' })
export class PersonService {
  private http = inject(HttpClient);
  private queryClient = inject(QueryClient);
  private queryService = inject(QueryService);

  fetchPeople() {
    return queryOptions({
      queryKey: [QueryKey.People],
      queryFn: async () => await lastValueFrom(
          this.queryService.fetchAllPeople()
        )
    });
  }

  fetchPersonByName(name: string) {
    return queryOptions({
      queryKey: [QueryKey.People, name],
      queryFn: async () => 
        lastValueFrom(
          this.queryService.fetchPersonByName(name)
        )
    });
  }

  addPerson() {
    return mutationOptions({
      mutationKey: [QueryKey.People],
      mutationFn: async (name: string) => {
        return await lastValueFrom(
          this.queryService.createPerson(name)
        );
      }, 
      onSuccess: async () => {
        await this.queryClient.invalidateQueries({ queryKey: [QueryKey.People] });
      }, 
      onError: (error: any) => {
        console.error('Error adding person:', error);
        throw error;
      }
    });
  }

  updatePerson() {
    return mutationOptions({
      mutationKey: [QueryKey.People],
      mutationFn: async (data: { name: string; newName: string }) => {
        return await lastValueFrom(
          this.queryService.updatePerson(data)
        );
      },
      onSuccess: async () => {
        await this.queryClient.invalidateQueries({ queryKey: [QueryKey.People] });
      },
      onError: (error: any) => {
        console.error('Error updating person:', error);
        throw error;
      }
    });
  }
}