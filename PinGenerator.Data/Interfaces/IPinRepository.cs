using PinGenerator.Model.Entities;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace PinGenerator.Data.Interfaces
{
    public interface IPinRepository
    {
        /// <summary>
        /// Determines whether the PIN DataStore has already been initialized by checking whether it contains contents. The assumption is that if any content exists, it has already been initialized.
        /// </summary>
        /// <returns>A boolean indicating whether the DataStore has been initialized.</returns>
        Task<bool> IsInitialized();

        /// <summary>
        /// Adds all the PIN objects in the given DataTable to the data store via a stored procedure on the database.
        /// </summary>
        /// <param name="pinsTable">A DataTable containing PIN data.</param>
        /// <returns>A boolean indicating whether the bulk insert succeeded.</returns>
        Task<bool> AddPins(DataTable pinsTable);

        /// <summary>
        /// Get the requested number of unallocated PINs from the DataStore. 
        /// </summary>
        /// <param name="requested">The number of PINs requested by the user</param>
        /// <returns>A list of PIN objects, equal in length to number requested</returns>
        Task<IReadOnlyList<PIN>> GetPINs(int requested);

        /// <summary>
        /// Will reset allocation status of PINs when all PINs have been allocated.
        /// </summary>
        /// <returns>A boolean value indicating successful reset of PINs</returns>
        Task<bool> ResetPINs();
    }
}
