import { TestBed } from '@angular/core/testing';

import { AstronautDutyService } from './astronaut-duty.service';

describe('AstronautDutyService', () => {
  let service: AstronautDutyService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AstronautDutyService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
