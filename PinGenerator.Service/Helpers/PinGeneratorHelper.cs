using PinGenerator.Data.Interfaces;
using PinGenerator.Model.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PinGenerator.Service.Helpers
{
    public static class PinGeneratorHelper
    {
        /// <summary>
        /// Requests the specified number of PINs still needed from the DataStore. 
        /// If the number of PINs returned is less than expected, tells the DataStore to reset the allocation status of all PINs and recursively calls itself to retrieve the remaining required PINs.
        /// </summary>
        /// <param name="requested">The number of PINs still required to complete the user's request.</param>
        /// <param name="pins">The list of PINs that have already been returned by the DataStore.</param>
        /// <returns>A list of PINs that is a combination of previously retreived and newly retreived pins.</returns>
        public static async Task<IReadOnlyList<PIN>> HandlePINRetrieval(IUnitOfWork unitOfWork, int requested, IReadOnlyList<PIN> pins)
        {
            pins = await AddPINsFromDataStore(unitOfWork, requested, pins);

            if (RequestNotFulfilled(requested, pins))
            {
                await unitOfWork.Pins.ResetPINs();
                pins = await HandlePINRetrieval(unitOfWork, (requested - pins.Count), pins);
            }

            return pins;
        }

        /// <summary>
        /// Requests the specified number of PINs still needed from the DataStore. 
        /// </summary>
        /// <param name="requested">The number of PINs still required to complete the user's request.</param>
        /// <param name="pins">The list of PINs that have already been returned by the DataStore.</param>
        /// <returns>A list of PINs that is a combination of previously retreived and newly retreived pins.</returns>
        public static async Task<IReadOnlyList<PIN>> AddPINsFromDataStore(IUnitOfWork unitOfWork, int requested, IReadOnlyList<PIN> pins)
        {
            pins = pins.Concat(await unitOfWork.Pins.GetPINs(requested)).ToList();

            return pins;
        }

        /// <summary>
        /// Determines whether the current request has been fulfilled, based on whether the requested number of PINs have been added to the PIN list to be returned.
        /// </summary>
        /// <param name="requested">The number of PINs still required to complete the user's request.</param>
        /// <param name="pins">The list of PINs that have already been returned by the DataStore.</param>
        /// <returns>A boolean indicating whether the current request is complete.</returns>
        public static bool RequestNotFulfilled(int requested, IReadOnlyList<PIN> pins)
        {
            return pins.Count() < requested;
        }

        /// <summary>
        /// Generates and stores all valid PIN options.
        /// </summary>
        /// <returns>A boolean value indicating process completion.</returns>
        public static async Task<bool> FirstTimePinGenerationAsync(IUnitOfWork unitOfWork)
        {
            List<PIN> pinList = GetAllPINs();
            DataTable pinListDT = ConvertToDataTable(pinList);

            return await unitOfWork.Pins.AddPins(pinListDT);
        }

        /// <summary>
        /// Generates all valid PIN options.
        /// </summary>
        /// <returns>A list containing all possible PIN options.</returns>
        public static List<PIN> GetAllPINs()
        {
            IEnumerable<string> allPossiblePins = GetAllPossiblePins();
            IEnumerable<string> allValidPins = GetAllValidPins(allPossiblePins);

            return MapToPINList(allValidPins);
        }

        /// <summary>
        /// Generate a list of all possible PIN options, ignoring whether they are valid or not.
        /// </summary>
        /// <returns>A list of all possible PIN options.</returns>
        public static IEnumerable<string> GetAllPossiblePins()
        {
            return Enumerable.Range(0, 10000)
                             .Select(x => x.ToString()
                                           .PadLeft(4, '0')
                                    );
        }

        /// <summary>
        /// Flags and removes all invalid PINs from the given list of PINs
        /// </summary>
        /// <param name="allPossiblePins">A list of all possible PINs which may contain invalid PINs to be removed.</param>
        /// <returns>A list of all valid PINs.</returns>
        public static IEnumerable<string> GetAllValidPins(IEnumerable<string> allPossiblePins)
        {
            return allPossiblePins.Where(pin => IsValidPin(pin));
        }

        /// <summary>
        /// Maps the provided list of PIN strings to a usable list of PIN objects.
        /// </summary>
        /// <param name="allValidPins">A list containing all the PIN strings to be mapped</param>
        /// <returns>A list containing PIN objects to be stored.</returns>
        public static List<PIN> MapToPINList(IEnumerable<string> allValidPins)
        {
            return allValidPins.Select(pin =>
                new PIN()
                {
                    PinString = pin,
                    Allocated = false
                }
            )
            .ToList();
        }

        /// <summary>
        /// Determines whether a given PIN string is valid or not (i.e. whether it is too obvious). Some of these choices are influenced by the following post: https://www.datagenetics.com/blog/september32012/
        /// </summary>
        /// <param name="pin">The PIN to be classified as valid or invalid.</param>
        /// <returns>A boolean indicating whether the PIN is valid.</returns>
        public static bool IsValidPin(string pin)
        {
            return !IsSequential(pin)
                   && !ContainsPairs(pin)
                   && !ContainsRepeat(pin)
                   && !IsLogicalYear(pin)
                   && !IsPalindromic(pin);
        }

        /// <summary>
        /// Determines whether a given PIN string is equivalent to a human readable year in the range 1900-2029.
        /// </summary>
        /// <param name="pin">The PIN to be classified as being a logical year.</param>
        /// <returns>A boolean indicating whether the PIN is a logical year.</returns>
        public static bool IsLogicalYear(string pin)
        {
            int pinValue = Int32.Parse(pin);
            return (pinValue >= 1900 && pinValue <= 2029);
        }

        /// <summary>
        /// Determines whether a given PIN is palindromic in nature.
        /// </summary>
        /// <param name="pin">The PIN to be classified as being palindromic.</param>
        /// <returns>A boolean indicating whether the PIN is a palindromic string.</returns>
        public static bool IsPalindromic(string pin)
        {
            return pin.SequenceEqual(pin.Reverse());
        }

        /// <summary>
        /// Determins whether a given PIN contains only values in an sequence.
        /// </summary>
        /// <param name="pin">The PIN to be classified as being sequential.</param>
        /// <returns>A boolean indicating whether the PIN contains only sequential values.</returns>
        public static bool IsSequential(string pin)
        {
            return "0123456789".Contains(pin) || "9876543210".Contains(pin);
        }

        /// <summary>
        /// Determins whether a given PIN contains a repeated pattern. e.g. 2323 
        /// </summary>
        /// <param name="pin">The PIN to be classified as containing a repeated pattern.</param>
        /// <returns>A boolean indicating whether the PIN contains a repeated pattern.</returns>
        public static bool ContainsRepeat(string pin)
        {
            return pin.Substring(0, 2) == pin.Substring(2, 2);
        }

        /// <summary>
        /// Determins whether a given PIN contains only pairs. e.g. 4422
        /// </summary>
        /// <param name="pin">The PIN to be classified as containing only pairs.</param>
        /// <returns>A boolean indicating whether the PIN contains only paired values.</returns>
        public static bool ContainsPairs(string pin)
        {
            return (pin[0] == pin[1] && pin[2] == pin[3]);
        }

        /// <summary>
        /// Convert provided list of items to a DataTable, for processing in Dapper bulk inserts.
        /// </summary>
        /// <typeparam name="Pin">The type of the objects in the list which will be converted to a DataTable.</typeparam>
        /// <param name="data">A list of PIN objects of to be converted to a DataTable</param>
        /// <returns>A DataTable to be processed by Dapper.</returns>
        public static DataTable ConvertToDataTable<Pin>(IList<Pin> data)

        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Pin));

            System.Data.DataTable table = new System.Data.DataTable();

            foreach (PropertyDescriptor prop in properties)
            {
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (Pin item in data)

            {
                DataRow row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;

                }

                table.Rows.Add(row);
            }

            return table;

        }
    }
}
