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
            if (requestModel is { Surgery: true, SurgeryBudget: 0 } or { Dentistry: true, DentistryBudget: 0 } or { Hospitalization: true, HospitalizationBudget: 0 })
            {
                throw new Exception("You cant use a coverage without putting any budget for it.");
            }
            
            var request = new Request
            {
                Title = requestModel.Title
            };
            await db.Request.AddAsync(request);
            await db.SaveChangesAsync();

            if (requestModel.Surgery)
            {
                var surgery = new RequestType
                {
                    RequestId = request.Id,
                    CoverageId = (int)CoverageType.Surgery,
                    Budget = requestModel.SurgeryBudget,
                };
                await db.RequestType.AddAsync(surgery);
            }

            if (requestModel.Dentistry)
            {
                var dentistry = new RequestType
                {
                    RequestId = request.Id,
                    CoverageId = (int)CoverageType.Dentistry,
                    Budget = requestModel.DentistryBudget,
                };
                await db.RequestType.AddAsync(dentistry);
            }

            if (requestModel.Hospitalization)
            {
                var hospitalization = new RequestType
                {
                    RequestId = request.Id,
                    CoverageId = (int)CoverageType.Hospitalization,
                    Budget = requestModel.HospitalizationBudget,
                };
                await db.RequestType.AddAsync(hospitalization);
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