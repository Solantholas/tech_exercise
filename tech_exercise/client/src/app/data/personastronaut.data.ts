export interface PersonAstronaut {
  personId: number;
  name: string;
  currentRank?: string;
  currentDutyTitle?: string;
  careerStartDate?: string; // ISO date string
  careerEndDate?: string;   // ISO date string
}