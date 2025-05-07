/// <summary>
/// Item received from server.
/// </summary>
public class ServerItem
{
    public string _id { get; set; }
    public string name { get; set; }
    public string shape { get; set; }
    public int weight { get; set; }
    public string[] material { get; set; }
    public string recycling { get; set; }
    public string unityKey { get; set; }
    public string filename { get; set; }
    public int rowNumber { get; set; }
    public int columnNumber { get; set; }
    public bool isInStock { get; set; }
    public bool isFurniture { get; set; }
    public string stock_id { get; set; }
}
