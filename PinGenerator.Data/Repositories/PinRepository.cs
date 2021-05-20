using Dapper;
using Microsoft.Extensions.Configuration;
using PinGenerator.Data.Interfaces;
using PinGenerator.Model.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinGenerator.Data.Repositories
{
    public class PinRepository : IPinRepository
    {
        private readonly IConfiguration configuration;
        
        public PinRepository(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Determines whether the PIN DataStore has already been initialized by checking whether it contains contents. The assumption is that if any content exists, it has already been initialized.
        /// </summary>
        /// <returns>A boolean indicating whether the DataStore has been initialized.</returns>
        public async Task<bool> IsInitialized()
        {
            const string sql = "SELECT CASE WHEN COUNT(1) > 0 THEN 1 ELSE 0 END AS [Initialized] FROM PIN";

            using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await connection.QuerySingleOrDefaultAsync<bool>(sql);

                return result;
            }
        }

        /// <summary>
        /// Adds all the PIN objects in the given DataTable to the data store via a stored procedure on the database.
        /// </summary>
        /// <param name="pinsTable">A DataTable containing PIN data.</param>
        /// <returns>A boolean indicating whether the bulk insert succeeded.</returns>
        public async Task<bool> AddPins(DataTable pinsTable)
        {
            using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
            {
                var pinsTVP = new
                {
                    pinList = pinsTable.AsTableValuedParameter("PINValue")
                };

                var result = await connection.ExecuteAsync("[dbo].[sp_InsertPINs]", pinsTVP, commandType: CommandType.StoredProcedure);

                return result > 0;
            }
        }

        /// <summary>
        /// Get the requested number of unallocated PINs from the DataStore. 
        /// </summary>
        /// <param name="requested">The number of PINs requested by the user</param>
        /// <returns>A list of PIN objects, equal in length to number requested</returns>
        public async Task<IReadOnlyList<PIN>> GetPINs(int requested)
        {
            using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
            {
                var requestedSQLParam = new
                {
                    requested = requested
                };

                var results = await connection.QueryAsync<PIN>("[dbo].[sp_GetPINs]", requestedSQLParam, commandType: CommandType.StoredProcedure);

                return results.ToList();
            }
        }

        /// <summary>
        /// Will reset allocation status of PINs when all PINs have been allocated.
        /// </summary>
        /// <returns>A boolean value indicating successful reset of PINs</returns>
        public async Task<bool> ResetPINs()
        {
            const string sql = "UPDATE PIN SET Allocated = 0";

            using (var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection")))
            {
                var result = await connection.ExecuteAsync(sql);

                return result > 0;
            }
        }
    }
}
