using Microsoft.EntityFrameworkCore;
using TKV.Interface;
using TKV.Model.DbbContext;
using TKV.Model.DbModels;
using TKV.Model.JsonModels;

namespace TKV.Service;

public class MainServices(MyDbContext db)  : IMainServices
{
    public async Task<JsonResponse> CreateRequest(RequestModel requestModel)
    {
        try
        {
            if (requestModel.Surgery)
            {
                switch (requestModel.SurgeryBudget)
                {
                    case < 5000:
                        throw new Exception("You cant put less than 5000 budget for Surgery coverage.");
                    case > 500000000:
                        throw new Exception("You cant put more than 500000000 budget for Surgery coverage.");
                }
            }

            if (requestModel.Dentistry)
            {
                switch (requestModel.DentistryBudget)
                {
                    case < 4000:
                        throw new Exception("You cant put less than 4000 budget for Dentistry coverage.");
                    case > 400000000:
                        throw new Exception("You cant put more than 400000000 budget for Dentistry coverage.");
                }
            }

            if (requestModel.Hospitalization)
            {
                switch (requestModel.HospitalizationBudget)
                {
                    case < 2000:
                        throw new Exception("You cant put less than 2000 budget for Hospitalization coverage.");
                    case > 200000000:
                        throw new Exception("You cant put more than 200000000 budget for Hospitalization coverage.");
                }
            }
            
            var request = new Request
            {
                Title = requestModel.Title
            };
            await db.Request.AddAsync(request);
            await db.SaveChangesAsync();
            
            
            foreach (CoverageType coverageType in Enum.GetValues(typeof(CoverageType)))
            {
                var type = new RequestType
                {
                    RequestId = request.Id,
                    CoverageId = (int)coverageType,
                    Budget = coverageType switch
                    {
                        CoverageType.Surgery => requestModel.SurgeryBudget,
                        CoverageType.Dentistry => requestModel.DentistryBudget,
                        CoverageType.Hospitalization => requestModel.HospitalizationBudget,
                        _ => 0
                    }
                };
                await db.RequestType.AddAsync(type);
            }
            
            await db.SaveChangesAsync();
            return new JsonResponse { IsSuccess = true, Message = "Request has been saved successfully."};
        }
        catch (Exception e)
        {
            return new JsonResponse { IsSuccess = false, Message = e.Message};
        }
    }

    public async Task<JsonResponse> GetRequests()
    {
        try
        {
            var list = new List<RequestListModel>();
            var requests = await db.Request.ToListAsync();
            var requestTypes = await db.RequestType.ToListAsync();
            var coverages = await db.Coverage.ToListAsync();
            
            foreach (var request in requests)
            {
                var rlm = new RequestListModel
                {
                    Title = request.Title,
                    TotalNetPremium = 0
                };
                foreach (CoverageType coverageType in Enum.GetValues(typeof(CoverageType)))
                {
                    var type = requestTypes.FirstOrDefault(rt => rt.RequestId == request.Id && rt.CoverageId == (int)coverageType);
                    var coverage = coverages.FirstOrDefault(co => co.Id == (int)coverageType);
                    if (type == null || coverage == null) continue;
                    switch (coverageType)
                    {
                        case CoverageType.Surgery:
                            rlm.Surgery = true;
                            rlm.SurgeryBudget = type.Budget;
                            break;
                        case CoverageType.Dentistry:
                            rlm.Dentistry = true;
                            rlm.DentistryBudget = type.Budget;
                            break;
                        case CoverageType.Hospitalization:
                            rlm.Hospitalization = true;
                            rlm.HospitalizationBudget = type.Budget;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    rlm.TotalNetPremium += type.Budget * coverage.ProfitCoefficient;
                }

                list.Add(rlm);
            }
            await db.SaveChangesAsync();
            return new JsonResponse { IsSuccess = true, Message = "Request list has been loaded successfully.", Data = list};
        }
        catch (Exception e)
        {
            return new JsonResponse { IsSuccess = false, Message = e.Message};
        }
    }
}