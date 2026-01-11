using System;
using System.Collections.Generic;
using System.Drawing;

namespace LogicGatesSimulator.Core.Logic
{
    public interface ILogicGate
    {
        Point Location { get; set; }
        bool GetOutput();
        void SetValue(bool value);
        List<ILogicGate> Inputs { get; set; }
        List<ILogicGate> Outputs { get; set; }
    }
}