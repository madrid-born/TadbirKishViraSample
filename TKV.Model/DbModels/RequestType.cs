using System.ComponentModel.DataAnnotations.Schema;

namespace TKV.Model.DbModels;

public class RequestType
{
    public int Id { get; set; }
    [ForeignKey("Request")]
    public int RequestId { get; set; }
    [ForeignKey("Coverage")]
    public int CoverageId { get; set; }
    public double Budget { get; set; }
    public virtual Request? Request { get; set; }
    public virtual Coverage? Coverage { get; set; }

}