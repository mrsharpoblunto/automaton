using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomatonContract;

namespace AutomatonAgent
{
    class Task
    {
        public IAutomatonPlugin Plugin { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid Id { get; set; }
    }
}
