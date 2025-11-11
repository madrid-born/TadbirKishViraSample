namespace TKV.Model.DbModels;

public class Coverage
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public double ProfitCoefficient { get; set; }
}