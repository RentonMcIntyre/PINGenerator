using PinGenerator.Data.Interfaces;
using PinGenerator.Model.Entities;
using PinGenerator.Service.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PinGenerator.Service.Services
{
    public class PinGeneratorService : IPinGeneratorService
    {
        private readonly IUnitOfWork unitOfWork;

        public PinGeneratorService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Tests if PINs have been initialized and initializes them if not.
        /// </summary>
        /// <returns>A boolean value indicating whether initialisation has succeeded.</returns>
        public async Task<bool> InitializePins()
        {
            if (await unitOfWork.Pins.IsInitialized())
            {
                return true;
            }

            return await PinGeneratorHelper.FirstTimePinGenerationAsync(unitOfWork);
        }

        /// <summary>
        /// Queries the DataStore for the specified number of randomly selected PINs and ensures allocation is reset when necessary.
        /// </summary>
        /// <param name="requested">The number of PINs requested by the user.</param>
        /// <returns>A list of PIN objects equal in length to the requested number.</returns>
        public async Task<IReadOnlyList<PIN>> GetPINs(int requested)
        {
            IReadOnlyList<PIN> pins = new List<PIN>();

            pins = await PinGeneratorHelper.HandlePINRetrieval(unitOfWork, requested, pins);
            
            return pins;
        }
    }
}