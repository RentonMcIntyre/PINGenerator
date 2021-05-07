import { Injectable } from '@angular/core';
import { createClient, SupabaseClient } from '@supabase/supabase-js'
import { environment } from 'src/environments/environment';
import { Pin } from '../models/pin';

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
   * Returns a list of all PINs currently stored in data store
   */
  async getAllPins()
  {
  let { data: pins, error } = await this.db
                                  .from<Pin>(this.tableName)
                                  .select('*');

  return { pins, error }
  }

  /**
  * Adds all PIN options provided to the function to data store
  * @param addedPINs A list of Pin Objects to be added to the data store
  */
  async addPins(addedPINs: Pin[]) {
    const { data, error } = await this.db
                                  .from<Pin>(this.tableName)
                                  .insert(addedPINs);
    return { data, error };
  }

  async updatePins(updatedPins: Pin[]) {
    const { data, error } = await this.db
                                  .from<Pin>(this.tableName)
                                  .upsert(updatedPins);
    return { data, error }
  }
}
