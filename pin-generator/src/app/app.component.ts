import { Component } from '@angular/core';

export interface PinData {
  PIN: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  generatedPINs: PinData[] = [];
  displayedColumns: string[] = ['PIN'];
  requestedPINs: number = 1;
}
