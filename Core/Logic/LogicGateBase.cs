using System;
using System.Collections.Generic;
using System.Drawing;

namespace LogicGatesSimulator.Core.Logic
{
    public abstract class LogicGateBase : ILogicGate
    {
        public Point Location { get; set; }
        public List<ILogicGate> Inputs { get; set; } = new List<ILogicGate>();
        public List<ILogicGate> Outputs { get; set; } = new List<ILogicGate>();

        public abstract bool GetOutput();
        public abstract void SetValue(bool value);
    }
}