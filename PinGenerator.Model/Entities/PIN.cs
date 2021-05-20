using System;
using System.Collections.Generic;
using System.Text;

namespace PinGenerator.Model.Entities
{
    public class PIN
    {
        public int id { get; set; }
        public string PinString { get; set; }
        public bool Allocated { get; set; }
    }
}
