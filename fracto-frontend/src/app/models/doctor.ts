export interface Doctor {
  id?: number;
  name: string;
  city: string;
  specializationId: number | string;
  rating: number;
  startTime: string;            // UI "HH:mm" or API "HH:mm:ss"
  endTime: string;
  slotDurationMinutes: number;
  profileImagePath: string;
}
