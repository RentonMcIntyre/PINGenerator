using PinGenerator.Model.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PinGenerator.Service.Services
{
    public interface IPinGeneratorService
    {
        /// <summary>
        /// Tests if PINs have been initialized and initializes them if not.
        /// </summary>
        /// <returns>A boolean value indicating whether initialisation has succeeded.</returns>
        public Task<bool> InitializePins();

        /// <summary>
        /// Queries the DataStore for the specified number of randomly selected PINs and ensures allocation is reset when necessary.
        /// </summary>
        /// <param name="requested">The number of PINs requested by the user.</param>
        /// <returns>A list of PIN objects equal in length to the requested number.</returns>
        public Task<IReadOnlyList<PIN>> GetPINs(int requested);
    }
}