export interface Appointment {
  id?: number;
  userId: number;
  doctorId: number;
  appointmentDate: string;
  timeSlot: string;
  status?: string;
  doctor?: any;            // here we can have doctor details if needed
}
