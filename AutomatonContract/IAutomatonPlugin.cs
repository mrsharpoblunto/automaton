using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomatonContract
{
    public interface IAutomatonPlugin
    {
        string Name { get; }
        string Description { get; }
        void Run();
        void Load(List<string> parameters);
    }
}