using System.Collections.Generic;

public class FindSymbolicFurniture : DailyTaskProgressListener
{
    private List<string> _furnitureNames = new();

    public void FurniturePressed(string furnitureName)
    {
        if (!_furnitureNames.Contains(furnitureName))
            _furnitureNames.Add(furnitureName);

        if (_furnitureNames.Count >= 3)
        {
            UpdateProgress("1");
            _furnitureNames.Clear();
        }
    }
}
