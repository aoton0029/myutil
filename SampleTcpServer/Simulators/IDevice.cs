using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleTcpServer.Simulators
{
    interface IDevice
    {
        string Name { get; }
        string ProcessCommand(string command);
    }

    class PowerSupply : IDevice
    {
        public string Name { get; }
        private double _voltage = 0.0;
        private double _current = 1.0;

        public PowerSupply(string name)
        {
            Name = name;
        }

        public string ProcessCommand(string command)
        {
            string[] parts = command.Split(' ');
            switch (parts[0])
            {
                case "GET_VOLTAGE":
                    return $"VOLTAGE {_voltage}V";

                case "SET_VOLTAGE":
                    if (parts.Length > 1 && double.TryParse(parts[1], out double voltage))
                    {
                        _voltage = voltage;
                        return $"VOLTAGE SET TO {_voltage}V";
                    }
                    return "INVALID VOLTAGE VALUE";

                case "MEASURE_CURRENT":
                    return $"CURRENT {_current}A";

                case "SET_CURRENT":
                    if (parts.Length > 1 && double.TryParse(parts[1], out double current))
                    {
                        _current = current;
                        return $"CURRENT SET TO {_current}A";
                    }
                    return "INVALID CURRENT VALUE";

                case "GET_POWER":
                    return $"POWER {_voltage * _current}W";

                default:
                    return "UNKNOWN COMMAND";
            }
        }
    }

    class Multimeter : IDevice
    {
        public string Name { get; }
        private Random _random = new();

        public Multimeter(string name)
        {
            Name = name;
        }

        public string ProcessCommand(string command)
        {
            switch (command)
            {
                case "MEASURE_VOLTAGE":
                    return $"VOLTAGE {_random.NextDouble() * 20:F2}V"; // 0~20V のランダム値

                case "MEASURE_CURRENT":
                    return $"CURRENT {_random.NextDouble() * 5:F2}A"; // 0~5A のランダム値

                case "MEASURE_RESISTANCE":
                    return $"RESISTANCE {_random.Next(10, 1000)}Ω"; // 10Ω~1000Ω のランダム値

                default:
                    return "UNKNOWN COMMAND";
            }
        }
    }

    class TemperatureSensor : IDevice
    {
        public string Name { get; }
        private Random _random = new();

        public TemperatureSensor(string name)
        {
            Name = name;
        }

        public string ProcessCommand(string command)
        {
            switch (command)
            {
                case "GET_TEMPERATURE":
                    return $"TEMPERATURE {_random.Next(15, 35)}°C"; // 15~35°C のランダム温度

                case "GET_HUMIDITY":
                    return $"HUMIDITY {_random.Next(30, 80)}%"; // 30%~80% のランダム湿度

                default:
                    return "UNKNOWN COMMAND";
            }
        }
    }


}
