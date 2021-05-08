import {
  MatSnackBar,
  MatSnackBarHorizontalPosition,
  MatSnackBarVerticalPosition,
} from '@angular/material/snack-bar';

import { Component, OnInit, ViewChild } from '@angular/core';
import { Pin, PinState } from './models/pin';
import { SupabaseHelperService } from './services/supabase-helper.service';
import { DateAdapter } from '@angular/material/core';
import { MatTable } from '@angular/material/table';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  generatedPINs: Pin[] = [];
  displayedColumns: string[] = ['PIN'];
  requestedPINs: number = 1;
  isSetupComplete: boolean = false;

  horizontalPosition: MatSnackBarHorizontalPosition = 'end';
  verticalPosition: MatSnackBarVerticalPosition = 'bottom';

  @ViewChild(MatTable)
  table!: MatTable<any>;

  constructor(private supabaseService: SupabaseHelperService, private _snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.supabaseService.getAllPINs()
                        .then(data => {
                          if(data.error)
                          {
                            this.openSnackBar(data.error.message);
                            return;
                          }
                          this.handleInitialConfig(data);
                          this.isSetupComplete = true;
    });
  }

  /**
   * Fired upon clicking the "GENERATE" button. 
   * Uses service to retrieve the number of PINs requested by the user and wraps the handler for data reception
   */
  retrieveNewPINs(): void {
    this.supabaseService.getRandomPINs(this.requestedPINs)
                        .then(data => {
                          this.handleRandomPinReturn(data);
                        });
  }

  /**
   * Handles the reception of randomly selected PINs and determins whether an allocation rollover is necessary.
   * Assumes that in the unlikely event a user requested multiple times the total valid PINs, to retrieve a complete list of PINs could take multiple rollovers.
   * @param data An object containing the random selection of PINs returned by the database.
   */
  private handleRandomPinReturn(data: { pins: any[] | null; error: { message: string; details: string; hint: string; code: string; } | null; }) {
    if(data.error)
    {
      this.openSnackBar(data.error.message);
      return;
    }

    this.generatedPINs = (data?.pins ?? []);
    this.checkIfRolloverRequired();
  }

  /**
   * Checks if rollover is required and initiates rollover if necessary. 
   * Is called recursively, stopping when all PINs requested have been returned. 
   */
  private checkIfRolloverRequired() {
    if (this.generatedPINs.length < this.requestedPINs) {
      this.rolloverPinAllocation(this.requestedPINs - this.generatedPINs.length);
    }
  }

  /**
   * Handles the deallocation of all allocated PIN items when no unallocated PINs remain.
   * @param remainingRequiredPins The number of PINs still required to be generated to complete the user's request after all allocated PINs are deallocated.
   */
  private rolloverPinAllocation(remainingRequiredPins: number) {
    this.supabaseService.resetPinAllocation()
      .then(data => {
        if (data.error) {
          this.openSnackBar(data.error.message);
          return;
        }
        this.getRemainingRandomPINs(remainingRequiredPins);
      });
  }

  /**
   * Retrieves a list of PINs to add to the previously obtained PIN list, in the scenario where more PINs were requested than were available.
   * @param remainingRequiredPins The number of PINs still required to be generated to complete the user's request after all allocated PINs are deallocated.
   */
  private getRemainingRandomPINs(remainingRequiredPins: number) {
    this.supabaseService.getRandomPINs(remainingRequiredPins)
      .then(data => {
        if (data.error) {
          this.openSnackBar(data.error.message);
          return;
        }

        this.generatedPINs.push(...(data?.pins ?? []));
        this.table.renderRows();
        this.checkIfRolloverRequired();
      });
  }

  /**
   * Runs the initial setup for the PIN data store. 
   * If no PINs exist in supabase storage, create and add them. 
   * If none are flagged as NotAllowed, get all PINs deemed unsuitable for use and flag them on the data store.
   * This process will only add any significant time complexity on initial run and 
   * will henceforth allow the application to be significantly more efficient on subsequent runs and PIN generation.
   * @param data An object containing all PINs currently stored in the data store
   */
  private handleInitialConfig(data: { pins: Pin[] | null; error: { message: string; details: string; hint: string; code: string; } | null; count: number | null }): void {
    let pins: Pin[] = data.pins ?? [];

    if (data.count == 0) {
      pins = this.createNewPinEntries();
    }

    if (!this.notAllowedHasBeenMarked(data.pins as Pin[])) {
      this.supabaseService.updatePINs(
        this.generateInvalidPINs(pins)
      );
    }
  }

  /**
   * Create a list of PINs and add them to the data store
   * @returns A list of all PINs that have been created
   */
  private createNewPinEntries(): Pin[] {
    let addedPINs: Pin[] = this.generateNewPINs();
    this.supabaseService.addPINs(addedPINs)
      .then(response => {
        if (response.error) {
          this.openSnackBar(response.error.message);
          return;
        }
      });

    return addedPINs;
  }

  /**
   * Generates a list of PINs covering all valid options from 0000-9999
   * @returns A list of PINs from 0000-9999
   */
  private generateNewPINs(): Pin[] {
    let unformattedPINs: number[] = Array.from(Array(10000).keys());
    
    return unformattedPINs.map(pin => {
                                return {
                                  PIN: ('0000'+pin).slice(-4),
                                  State: PinState.Unallocated
                                } as Pin
                              });
  }

  /**
   * Determins whether any PIN exists that have been marked as NotAllowed
   * @param pins A list of PINs to be checked
   * @returns A boolean that indicates whether any of the provided PINs are marked NotAllowed
   */
  private notAllowedHasBeenMarked(pins: Pin[]): boolean {
    return pins?.some(x => x.State == PinState.NotAllowed);
  }

  /**
   * Generates a list of PINs that are not allowed to be returned to the user. Some of these choices are influenced by the following post: https://www.datagenetics.com/blog/september32012/
   * @param pins The list of unflagged PINs for reference
   * @returns A list of PINs to be marked as NotAllowed
   */
  private generateInvalidPINs(pins: Pin[]): Pin[] {
    let invalidPINs: Pin[] = pins.filter(x => 
                                          (x.PIN[0] === x.PIN[1] && x.PIN[2] == x.PIN[3])       ||             // Contains 2 sets of the same 2 values i.e. 5544
                                          (parseInt(x.PIN[0]) === parseInt(x.PIN[1])-1 && parseInt(x.PIN[0]) === parseInt(x.PIN[2])-2 
                                                && parseInt(x.PIN[0]) === parseInt(x.PIN[3])-3) ||             // Is ascending sequence i.e. 1234
                                          (parseInt(x.PIN[0]) === parseInt(x.PIN[1])+1 && parseInt(x.PIN[0]) === parseInt(x.PIN[2])+2 
                                                && parseInt(x.PIN[0]) === parseInt(x.PIN[3])+3) ||             // Is descending sequence i.e. 4321
                                          x.PIN == x.PIN.split("").reverse().join("")           ||             // Is palindromic i.e. 4334
                                          /^(19\d\d)|(200\d)|(201\d)|(202\d)|(\d\d)\5{1,}|(000\d)$/.test(x.PIN)        // Is a year that could be an valid human's birthyear or relevant in the next few years (1900->2029)
                                        );                                                                     // OR Contains 2 sets of a repeating 2 digit combo i.e. 2727
                                                                                                               // OR Is in the range 0001-0009, which are likely easy to brute force manually

    invalidPINs.forEach(x => x.State = PinState.NotAllowed);

    return invalidPINs;
  }
  
  /**
   * Displays a small message box primarily used for error reporting.
   * @param message The message to be displayed in the snackbar.
   */
  private openSnackBar(message: string) {
    const snackbarRef = this._snackBar.open(message, 'OK', {
      duration: 10000,
      horizontalPosition: this.horizontalPosition,
      verticalPosition: this.verticalPosition,
    });
  }
}