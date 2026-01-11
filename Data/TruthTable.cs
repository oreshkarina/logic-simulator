using System;
using System.Data;
using LogicGatesSimulator.Core.Simulation;

namespace LogicGatesSimulator.Data
{
    public class TruthTable
    {
        public DataTable Table { get; private set; }
        private Circuit circuit;

        public TruthTable(Circuit circuit)
        {
            this.circuit = circuit;
            CreateTableStructure();
        }

        private void CreateTableStructure()
        {
            Table = new DataTable("TruthTable");

            // Колонки для входов (максимум 5)
            int inputCount = Math.Min(circuit.Inputs.Count, 5);
            for (int i = 0; i < inputCount; i++)
            {
                Table.Columns.Add($"Вход {i + 1}", typeof(string));
            }

            // Колонки для выходов
            for (int i = 0; i < circuit.Outputs.Count; i++)
            {
                Table.Columns.Add($"Выход {i + 1}", typeof(string));
            }
        }

        public void GenerateAllCombinations()
        {
            Table.Rows.Clear();

            int inputCount = Math.Min(circuit.Inputs.Count, 5);
            if (inputCount == 0) return;

            // Генерируем все комбинации (2^inputCount)
            int totalCombinations = 1 << inputCount; // 2^inputCount

            for (int i = 0; i < totalCombinations; i++)
            {
                bool[] inputs = new bool[inputCount];
                for (int j = 0; j < inputCount; j++)
                {
                    // Преобразуем число в двоичный вид
                    inputs[j] = ((i >> (inputCount - 1 - j)) & 1) == 1;
                }

                // Симулируем схему
                bool[] outputs = circuit.Simulate(inputs);

                // Создаем строку таблицы
                DataRow row = Table.NewRow();

                // Заполняем входы (0 или 1)
                for (int j = 0; j < inputCount; j++)
                {
                    row[j] = inputs[j] ? "1" : "0";
                }

                // Заполняем выходы (0 или 1)
                for (int j = 0; j < outputs.Length; j++)
                {
                    row[inputCount + j] = outputs[j] ? "1" : "0";
                }

                Table.Rows.Add(row);
            }
        }

        public void AddRow(bool[] inputs, bool[] outputs)
        {
            int inputCount = Math.Min(inputs.Length, 5);

            DataRow row = Table.NewRow();

            // Заполняем входы
            for (int i = 0; i < inputCount; i++)
            {
                row[i] = inputs[i] ? "1" : "0";
            }

            // Заполняем выходы
            for (int i = 0; i < outputs.Length; i++)
            {
                row[inputCount + i] = outputs[i] ? "1" : "0";
            }

            Table.Rows.Add(row);
        }

        public void Clear()
        {
            Table.Rows.Clear();
        }
    }
}