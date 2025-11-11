using Microsoft.AspNetCore.Mvc;
using TKV.Interface;
using TKV.Model.JsonModels;

namespace TadbirKishViraSample.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MainController(IMainServices mainServices) : Controller
{
    [HttpPost("CreateRequest")]
    public async Task<IActionResult> CreateRequest([FromBody] RequestModel requestModel)
    {
        return Ok(await mainServices.CreateRequest(requestModel));
    }
    
    [HttpGet("GetRequests")]
    public async Task<IActionResult> GetRequests()
    {
        return Ok(await mainServices.GetRequests());
    }
}