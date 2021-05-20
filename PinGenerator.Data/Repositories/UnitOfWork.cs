using PinGenerator.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PinGenerator.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(IPinRepository pinRepository)
        {
            Pins = pinRepository;
        }

        public IPinRepository Pins { get; }
    }
}
