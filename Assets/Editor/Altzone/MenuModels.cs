using System.Linq;
using System.Text;
using Altzone.Scripts.Model;

namespace Editor.Altzone
{
    internal static class MenuModels
    {
        public static void DumpModelsToWikiTable()
        {
            Debug.Log("*");
            ModelLoader.LoadAndClearModels();
            var models = Models.GetAll<CharacterModel>();
            var builder = new StringBuilder();
            builder
                .Append("| Nimi | Defenssi | Speed | Resistance | Attack | Defence |").AppendLine()
                .Append("|:---- |:-------- |:-----:|:----------:|:------:|:-------:|").AppendLine();
            foreach (var m in models.OrderBy(x => x.Id))
            {
                builder
                    .Append($"| {m.Name} | {m.MainDefence} | {m.Speed} | {m.Resistance} | {m.Attack} | {m.Defence} |").AppendLine();
            }
            Debug.Log($"(needs to scroll down to see the table)\r\nModels\r\n{builder}");
        }
    }
}