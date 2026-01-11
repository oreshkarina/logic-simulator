using System;
using System.Drawing;

namespace LogicGatesSimulator.Core.Simulation
{
    public class InputGate : LogicGatesSimulator.Core.Logic.LogicGateBase
    {
        private bool value = false;
        private string name;

        public InputGate(string name)
        {
            this.name = name;
        }

        public override bool GetOutput()
        {
            return value;
        }

        public override void SetValue(bool value)
        {
            this.value = value;
        }
    }
}