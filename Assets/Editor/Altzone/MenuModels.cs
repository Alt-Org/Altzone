using System.IO;
using System.Linq;
using System.Text;
using Altzone.Scripts.Model;
using Altzone.Scripts.Model.Dto;
using UnityEngine;

namespace Editor.Altzone
{
    internal static class MenuModels
    {
        public static void DumpModelsToWikiTable()
        {
            Debug.Log("*");
            Models.Load();
            var models = Models.GetAll<CharacterClassModel>();
            var builder = new StringBuilder();
            var speedStats = new Stats();
            var resistanceStats = new Stats();
            var attackStats = new Stats();
            var defenceStats = new Stats();
            builder
                .Append("| Nimi | Defenssi | Speed | Resistance | Attack | Defence |").AppendLine()
                .Append("|:---- |:-------- |:-----:|:----------:|:------:|:-------:|").AppendLine();
            foreach (var m in models.OrderBy(x => x.Id))
            {
                builder
                    .Append($"| {m.Name} | {m.MainDefence} | {m.Speed} | {m.Resistance} | {m.Attack} | {m.Defence} |").AppendLine();
                speedStats.Add(m.Speed);
                resistanceStats.Add(m.Resistance);
                attackStats.Add(m.Attack);
                defenceStats.Add(m.Defence);
            }
            builder
                .Append($"| Min | &nbsp; | {speedStats.Min} | {resistanceStats.Min} | {attackStats.Min} | {defenceStats.Min} |").AppendLine();
            builder
                .Append($"| Max | &nbsp; | {speedStats.Max} | {resistanceStats.Max} | {attackStats.Max} | {defenceStats.Max} |").AppendLine();
            builder
                .Append($"| Delta | &nbsp; | {speedStats.Delta} | {resistanceStats.Delta} | {attackStats.Delta} | {defenceStats.Delta} |").AppendLine();
            Debug.Log($"(needs to scroll down to see the table)\r\nModels\r\n{builder}");
        }

        private class Stats
        {
            public int Min { get; private set; } = int.MaxValue;
            public int Max { get; private set; } = int.MinValue;
            public int Delta => Max - Min;

            public void Add(int value)
            {
                if (value < Min)
                {
                    Min = value;
                }
                if (value > Max)
                {
                    Max = value;
                }
            }
        }
    }
}