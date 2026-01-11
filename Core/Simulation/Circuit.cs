using System;
using System.Collections.Generic;
using System.Linq;
using LogicGatesSimulator.Core.Logic;
using LogicGatesSimulator.Core.Models;

namespace LogicGatesSimulator.Core.Simulation
{
    public class Circuit
    {
        public List<ILogicGate> Gates { get; private set; } = new List<ILogicGate>();
        public List<Connection> Connections { get; private set; } = new List<Connection>();
        public List<InputGate> Inputs => Gates.OfType<InputGate>().ToList();
        public List<OutputGate> Outputs => Gates.OfType<OutputGate>().ToList();

        public void AddGate(ILogicGate gate)
        {
            if (!Gates.Contains(gate))
            {
                Gates.Add(gate);
            }
        }

        public void RemoveGate(ILogicGate gate)
        {
            // Удаляем все соединения с этим элементом
            var connectionsToRemove = Connections
                .Where(c => c.SourceGate == gate || c.TargetGate == gate)
                .ToList();

            foreach (var connection in connectionsToRemove)
            {
                RemoveConnection(connection);
            }

            Gates.Remove(gate);
        }

        private void RemoveConnection(Connection connection)
        {
            // Удаляем связи между элементами
            if (connection.SourceGate != null)
            {
                connection.SourceGate.Outputs.Remove(connection.TargetGate);
            }

            if (connection.TargetGate != null)
            {
                connection.TargetGate.Inputs.Remove(connection.SourceGate);
            }

            Connections.Remove(connection);
        }

        public void ConnectGates(ILogicGate source, ILogicGate target)
        {
            // Проверяем, не существует ли уже такое соединение
            if (!Connections.Any(c => c.SourceGate == source && c.TargetGate == target))
            {
                var connection = new Connection(source, target);
                Connections.Add(connection);

                // Добавляем связи между элементами
                if (!source.Outputs.Contains(target))
                    source.Outputs.Add(target);

                if (!target.Inputs.Contains(source))
                    target.Inputs.Add(source);
            }
        }

        public bool[] Simulate(bool[] inputValues)
        {
            if (inputValues.Length != Inputs.Count)
            {
                throw new ArgumentException(
                    $"Количество входных значений ({inputValues.Length}) " +
                    $"не совпадает с количеством входных элементов ({Inputs.Count})");
            }

            // Устанавливаем значения входов
            for (int i = 0; i < Inputs.Count; i++)
            {
                Inputs[i].SetValue(inputValues[i]);
            }

            // Вычисляем выходные значения
            List<bool> results = new List<bool>();

            foreach (var output in Outputs)
            {
                results.Add(output.GetOutput());
            }

            return results.ToArray();
        }

        // Вспомогательный метод для отладки
        public void PrintCircuitState()
        {
            Console.WriteLine($"Всего элементов: {Gates.Count}");
            Console.WriteLine($"Входы: {Inputs.Count}, Выходы: {Outputs.Count}, Соединений: {Connections.Count}");

            foreach (var gate in Gates)
            {
                string type = gate.GetType().Name;
                string inputs = string.Join(", ", gate.Inputs.Select(g => GetGateDisplayName(g)));
                string outputs = string.Join(", ", gate.Outputs.Select(g => GetGateDisplayName(g)));

                Console.WriteLine($"{GetGateDisplayName(gate)} ({type}): " +
                                 $"Inputs=[{inputs}], Outputs=[{outputs}], Value={gate.GetOutput()}");
            }
        }

        private string GetGateDisplayName(ILogicGate gate)
        {
            if (gate == null) return "null";

            if (gate is InputGate input)
            {
                int index = Inputs.IndexOf(input);
                return $"IN {index + 1}";
            }
            if (gate is OutputGate output)
            {
                int index = Outputs.IndexOf(output);
                return $"OUT {index + 1}";
            }
            if (gate is AndGate) return "AND";
            if (gate is OrGate) return "OR";
            if (gate is NotGate) return "NOT";
            if (gate is XorGate) return "XOR";
            return "GATE";
        }
    }
}