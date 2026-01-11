using System;
using System.Linq;

namespace LogicGatesSimulator.Core.Logic
{
    public class OrGate : LogicGateBase
    {
        public override bool GetOutput()
        {
            if (Inputs.Count == 0) return false;

            bool result = Inputs[0].GetOutput();
            for (int i = 1; i < Inputs.Count; i++)
            {
                result = result || Inputs[i].GetOutput();
            }
            return result;
        }

        public override void SetValue(bool value)
        {
            // Для базовых вентилей этот метод не используется напрямую
        }
    }
}