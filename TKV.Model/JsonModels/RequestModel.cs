namespace TKV.Model.JsonModels;

public class RequestModel
{
    public string Title { get; set; } = null!;
    public bool Surgery {get; set;} = false;
    public bool Dentistry {get; set;} = false;
    public bool Hospitalization {get; set;} = false;
    public double  SurgeryBudget { get; set; } = 0.0;
    public double  DentistryBudget { get; set; } = 0.0;
    public double  HospitalizationBudget { get; set; } = 0.0;
}