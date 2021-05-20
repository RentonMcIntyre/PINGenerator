import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Pin } from '../models/pin';
import { environment } from "../../environments/environment";

@Injectable({
  providedIn: 'root'
})
export class PinGeneratorService {

  constructor(private http: HttpClient) { }

  initializePins(): Observable<boolean> {
    return this.http.get<boolean>(environment.API_URL + "/initialize");
  }

  getPins(requested: number): Observable<Pin[]> {
    return this.http.get<Pin[]>(environment.API_URL + "/get-pins/" + requested);
  }
}
