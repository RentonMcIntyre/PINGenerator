import { Component, OnInit } from '@angular/core';
import { Pin, PinState } from './models/pin';
import { SupabaseHelperService } from './services/supabase-helper.service';

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

  constructor(private supabaseService: SupabaseHelperService) {}

  ngOnInit(): void {
    this.supabaseService.getAllPINs()
                        .then(data => {
                          if(data.error)
                          {
                            console.log(data.error);
                            return;
                          }
                          this.handleInitialConfig(data);
                          this.isSetupComplete = true;
    });
  }

  retrieveNewPINs(): void {
    this.supabaseService.getRandomPins(this.requestedPINs)
                        .then(data => {
                          if(data.error)
                          {
                            console.log(data.error);
                            return;
                          }

                          this.generatedPINs = (data?.pins ?? []);

                          while((data.pins?.length ?? 0) < this.requestedPINs)
                          {
                            console.log(data.pins?.length);
                            this.supabaseService.getRandomPins(this.requestedPINs)
                                                .then(data => {
                                                  if(data.error)
                                                  {
                                                    console.log(data.error);
                                                    return;
                                                  }
                                                  this.generatedPINs = (data?.pins ?? []);
                                                })
                          }
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

    if (!this.NotAllowedHasBeenMarked(data.pins as Pin[])) {
      this.supabaseService.updatePINs(
        this.GenerateInvalidPINs(pins)
      );
    }
  }

  /**
   * Create a list of PINs and add them to the data store
   * @returns A list of all PINs that have been created
   */
  private createNewPinEntries(): Pin[] {
    let addedPINs: Pin[] = this.GenerateNewPINs();
    this.supabaseService.addPINs(addedPINs)
      .then(response => {
        if (response.error) {
          console.log(response.error);
          return;
        }
      });

    return addedPINs;
  }

  /**
   * Generates a list of PINs covering all valid options from 0000-9999
   * @returns A list of PINs from 0000-9999
   */
  private GenerateNewPINs(): Pin[] {
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
  private NotAllowedHasBeenMarked(pins: Pin[]): boolean {
    return pins?.some(x => x.State == PinState.NotAllowed);
  }

  /**
   * Generates a list of PINs that are not allowed to be returned to the user. Some of these choices are influenced by the following post: https://www.datagenetics.com/blog/september32012/
   * @param pins The list of unflagged PINs for reference
   * @returns A list of PINs to be marked as NotAllowed
   */
  private GenerateInvalidPINs(pins: Pin[]): Pin[] {
    let invalidPINs: Pin[] = pins.filter(x => 
                                          (x.PIN[0] === x.PIN[1] && x.PIN[2] == x.PIN[3])       ||             // Contains 2 sets of the same 2 values i.e. 5544
                                          (parseInt(x.PIN[0]) === parseInt(x.PIN[1])-1 && parseInt(x.PIN[0]) === parseInt(x.PIN[2])-2 
                                                && parseInt(x.PIN[0]) === parseInt(x.PIN[3])-3) ||             // Is ascending sequence i.e. 1234
                                          (parseInt(x.PIN[0]) === parseInt(x.PIN[1])+1 && parseInt(x.PIN[0]) === parseInt(x.PIN[2])+2 
                                                && parseInt(x.PIN[0]) === parseInt(x.PIN[3])+3) ||             // Is descending sequence i.e. 4321
                                          x.PIN == x.PIN.split("").reverse().join("")           ||             // Is palindromic i.e. 4334
                                          /^(19\d\d)|(200\d)|(201\d)|(202\d)|(\d\d)\5{1,}$/.test(x.PIN)        // Is a year that could be an valid human's birthyear or relevant in the next few years (1900->2029)
                                        );                                                                     // OR Contains 2 sets of a repeating 2 digit combo i.e. 2727

    invalidPINs.forEach(x => x.State = PinState.NotAllowed);

    return invalidPINs;
  }
}