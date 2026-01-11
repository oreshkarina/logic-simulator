using System;
using System.Linq;

namespace LogicGatesSimulator.Core.Logic
{
    public class XorGate : LogicGateBase
    {
        public override bool GetOutput()
        {
            if (Inputs.Count == 0) return false;
            if (Inputs.Count == 1) return Inputs[0].GetOutput();

            bool result = Inputs[0].GetOutput();
            for (int i = 1; i < Inputs.Count; i++)
            {
                result = result ^ Inputs[i].GetOutput();
            }
            return result;
        }

        public override void SetValue(bool value)
        {
            // Для базовых вентилей этот метод не используется напрямую
        }
    }
}