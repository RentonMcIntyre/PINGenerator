export enum PinState {
  Unallocated = 0,
  Allocated = 1,
  NotAllowed = 2
}

export interface Pin {
    id: string;
    PIN: string;
    State: PinState
 }