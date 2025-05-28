import { Injectable, inject } from '@angular/core';
import { PersonAstronaut } from '../data/personastronaut.data';
import { urlsData } from '../data/urls.data';
import { HttpClient } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { mutationOptions, QueryClient, queryOptions } from '@tanstack/angular-query-experimental';
import { AstronautDuty } from '../data/astronaut-duty.data';

@Injectable({
  providedIn: 'root'
})
export class AstronautDutyService {
  private http = inject(HttpClient);
  private queryClient = inject(QueryClient);

  createAstronautDuties() {
    return mutationOptions({
      mutationKey: ['updateAstronautDuties'],
      mutationFn: async (data: AstronautDuty) => {
        return await lastValueFrom(
          this.http.post<any>(urlsData.createAstronautDuty, JSON.stringify(data), {
            headers: { 'Content-Type': 'application/json' }
          })
        );
      },
      onSuccess: async () => {
        await this.queryClient.invalidateQueries({ queryKey: ['people'] });
      },
      onError: (error: any) => {
        console.error('Error updating astronaut duties:', error);
        throw error;
      }
    });
  }
}