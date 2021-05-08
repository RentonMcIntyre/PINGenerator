import { Injectable } from '@angular/core';
import { createClient, SupabaseClient } from '@supabase/supabase-js'
import { environment } from 'src/environments/environment';
import { Pin, PinState } from '../models/pin';

@Injectable({
  providedIn: 'root'
})
export class SupabaseHelperService {
  db: SupabaseClient;
  tableName: string = "PIN";

  constructor() {
    this.db = createClient(environment.SUPABASE_URL, environment.SUPABASE_KEY);
  }

  /**
   * Returns a list of all PINs currently stored in data store.
   * @returns A list of all PINs in the data store, as well as the quantity
   */
  async getAllPINs()
  {
    let { data: pins, count, error } = await this.db
                                    .from<Pin>(this.tableName)
                                    .select('*', { count:  'exact' }  );

    return { pins, error, count }
  }

  /**
  * Adds all PIN options provided to the function to data store
  * @param addedPINs A list of Pin Objects to be added to the data store
  */
  async addPINs(addedPINs: Pin[]) {
    const { data, error } = await this.db
                                  .from<Pin>(this.tableName)
                                  .insert(addedPINs);
    return { data, error };
  }

  /**
   * Queries the data store to retrieve a specified number of random PINs.
   * Function on database handles selection of random PINs, prevention of reuse and rollover when all PINs have been allocated.
   * See src/assets for reference on procedure implementation. 
   * @param requestedPINs The number of random PINs requested
   * @returns A list of PINs
   */
  async getRandomPINs(requestedPINs: number)
  {
    let { data: pins, error } = await this.db
                                      .rpc<Pin>('getrandompins', {
                                        quantity: requestedPINs
                                      });

    return { pins, error };
  }

  /**
   * Executes a stored procedure on the database in order to reset all allocated pins.
   * For usage once all pins have been allocated.
   * See src/assets for reference on procedure implementation. 
   */
  async resetPinAllocation()
  {
    let { data, error } = await this.db
                                .rpc('resetpinallocation');

    return { data, error };
  }
}
