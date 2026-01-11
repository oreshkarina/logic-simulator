using System;
using System.Linq;

namespace LogicGatesSimulator.Core.Logic
{
    public class NotGate : LogicGateBase
    {
        public override bool GetOutput()
        {
            if (Inputs.Count == 0) return true; // Нет входа = всегда 1
            return !Inputs[0].GetOutput();
        }

        public override void SetValue(bool value)
        {
            // Для базовых вентилей этот метод не используется напрямую
        }
    }
}