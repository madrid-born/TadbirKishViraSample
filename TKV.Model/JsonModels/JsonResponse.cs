namespace TKV.Model.JsonModels;

public class JsonResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}