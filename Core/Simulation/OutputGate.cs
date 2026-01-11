using System;
using System.Drawing;
using System.Linq;

namespace LogicGatesSimulator.Core.Simulation
{
    public class OutputGate : LogicGatesSimulator.Core.Logic.LogicGateBase
    {
        private string name;

        public OutputGate(string name)
        {
            this.name = name;
        }

        public override bool GetOutput()
        {
            // Выход берет значение от своего входа
            if (Inputs.Count == 0) return false;
            return Inputs[0].GetOutput();
        }

        public override void SetValue(bool value)
        {
            // OutputGate не устанавливает значение напрямую
        }
    }
}