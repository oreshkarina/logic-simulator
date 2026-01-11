using LogicGatesSimulator.Core.Logic;

namespace LogicGatesSimulator.Core.Models
{
    public class Connection
    {
        public ILogicGate SourceGate { get; set; }
        public ILogicGate TargetGate { get; set; }

        public Connection(ILogicGate source, ILogicGate target)
        {
            SourceGate = source;
            TargetGate = target;
        }
    }
}