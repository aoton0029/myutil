using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    // SerialNumber.cs
    public class SerialNumber : IEquatable<SerialNumber>, ICloneable, IComparable<SerialNumber>
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public int Number { get; set; }

        public SerialNumber(string prefix, int number, string suffix)
        {
            if (number < 0 || number > 999999)
                throw new ArgumentOutOfRangeException(nameof(number), "Number must be between 0 and 999999.");

            Prefix = prefix ?? string.Empty;
            Suffix = suffix ?? string.Empty;
            Number = number;
        }

        public override string ToString() => $"{Prefix}{Number:D6}{Suffix}";

        public static SerialNumber Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 6)
                throw new FormatException("Invalid serial number format.");

            int start = -1;
            for (int i = 0; i <= input.Length - 6; i++)
            {
                if (int.TryParse(input.Substring(i, 6), out _))
                {
                    start = i;
                    break;
                }
            }

            if (start == -1)
                throw new FormatException("No 6-digit number found in the input.");

            string prefix = input.Substring(0, start);
            string numberStr = input.Substring(start, 6);
            string suffix = input.Substring(start + 6);

            return new SerialNumber(prefix, int.Parse(numberStr), suffix);
        }

        public bool Equals(SerialNumber? other)
            => other != null && Prefix == other.Prefix && Number == other.Number && Suffix == other.Suffix;

        public override bool Equals(object? obj) => Equals(obj as SerialNumber);

        public override int GetHashCode() => HashCode.Combine(Prefix, Number, Suffix);

        public object Clone() => new SerialNumber(Prefix, Number, Suffix);

        public int CompareTo(SerialNumber? other)
        {
            if (other == null) return 1;

            int prefixComp = string.Compare(Prefix, other.Prefix, StringComparison.Ordinal);
            if (prefixComp != 0) return prefixComp;

            int numberComp = Number.CompareTo(other.Number);
            if (numberComp != 0) return numberComp;

            return string.Compare(Suffix, other.Suffix, StringComparison.Ordinal);
        }
    }

    // SerialNumberListExtensions.cs
    public static class SerialNumberListExtensions
    {
        public static IEnumerable<SerialNumber> Generate(string prefix, int startNumber, string suffix, int? count = null)
        {
            if (startNumber < 0 || startNumber > 999999)
                throw new ArgumentOutOfRangeException(nameof(startNumber), "Start number must be between 0 and 999999.");

            int current = startNumber;
            int generated = 0;

            while (!count.HasValue || generated < count.Value)
            {
                if (current > 999999)
                    yield break;

                yield return new SerialNumber(prefix, current, suffix);
                current++;
                generated++;
            }
        }

        public static bool IsSequentialSamePrefixAndSuffix(this IEnumerable<SerialNumber> list)
        {
            var sorted = list.OrderBy(sn => sn.Number).ToList();
            if (!sorted.Any()) return false;

            var first = sorted.First();
            return sorted.All(sn => sn.Prefix == first.Prefix && sn.Suffix == first.Suffix) &&
                   sorted.Zip(sorted.Skip(1), (a, b) => b.Number - a.Number).All(diff => diff == 1);
        }

        public static Dictionary<string, List<SerialNumber>> GroupBySuffix(this IEnumerable<SerialNumber> list)
        {
            return list.GroupBy(sn => sn.Suffix)
                       .ToDictionary(g => g.Key, g => g.ToList());
        }

        public static string ToSerialString(this IEnumerable<SerialNumber> list)
        {
            var sorted = list.OrderBy(sn => sn.Number).ToList();
            if (!sorted.Any()) return string.Empty;

            List<string> result = new();
            int start = 0;

            while (start < sorted.Count)
            {
                int end = start;
                while (end + 1 < sorted.Count &&
                       sorted[end + 1].Number == sorted[end].Number + 1 &&
                       sorted[end + 1].Prefix == sorted[start].Prefix &&
                       sorted[end + 1].Suffix == sorted[start].Suffix)
                {
                    end++;
                }

                if (end > start)
                {
                    result.Add($"{sorted[start]}〜{sorted[end]}");
                    start = end + 1;
                }
                else
                {
                    result.Add(sorted[start].ToString());
                    start++;
                }
            }

            return string.Join(", ", result);
        }
    }

    // SerialNumberSegmentConfig.cs
    public class SerialNumberSegmentConfig
    {
        public int Count { get; set; }
        public bool Sort { get; set; }

        public SerialNumberSegmentConfig(int count, bool sort)
        {
            Count = count;
            Sort = sort;
        }
    }

    // SerialNumberListManager.cs
    public class SerialNumberListManager
    {
        public List<SerialNumber> SourceList { get; private set; } = new();
        public List<SerialNumberSegmentConfig> SegmentConfigs { get; set; } = new();

        public SerialNumberListManager(IEnumerable<SerialNumber> list)
        {
            SourceList = list.ToList();
        }

        public List<List<SerialNumber>> Split()
        {
            var result = new List<List<SerialNumber>>();
            int index = 0;

            foreach (var config in SegmentConfigs)
            {
                if (index >= SourceList.Count) break;

                var segment = SourceList.Skip(index).Take(config.Count).ToList();
                if (config.Sort)
                    segment = segment.OrderBy(sn => sn.Number).ToList();

                result.Add(segment);
                index += config.Count;
            }

            return result;
        }

        public string SaveState()
        {
            var state = new SerialNumberListManagerState
            {
                SourceList = SourceList,
                SegmentConfigs = SegmentConfigs
            };

            return JsonSerializer.Serialize(state);
        }

        public void LoadState(string json)
        {
            var state = JsonSerializer.Deserialize<SerialNumberListManagerState>(json);
            if (state != null)
            {
                SourceList = state.SourceList ?? new();
                SegmentConfigs = state.SegmentConfigs ?? new();
            }
        }

        private class SerialNumberListManagerState
        {
            public List<SerialNumber> SourceList { get; set; } = new();
            public List<SerialNumberSegmentConfig> SegmentConfigs { get; set; } = new();
        }
    }

}
