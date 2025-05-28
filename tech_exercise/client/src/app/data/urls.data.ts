import { environment } from '../environments/environment';

const urls = {
  astronautDuty: `${environment.apiUrl}/astronautduty`,
  astronautDutyByName: (name: string) => `${environment.apiUrl}/astronautduty/${encodeURIComponent(name)}`,
  person: `${environment.apiUrl}/person`,
  personByName: (name: string) => `${environment.apiUrl}/person/${encodeURIComponent(name)}`
};

export const urlsData = {
  fetchPeople: urls.person,
  fetchPersonByName: (name: string) => urls.personByName(name),
  createPerson: urls.person,
  updatePerson: urls.person,
  fetchAstronautDutyByName: (name: string) => urls.astronautDutyByName(name),
  createAstronautDuty: urls.astronautDuty,
};