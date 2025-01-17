using Microsoft.AspNetCore.Mvc;
using Api.Repository;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ApplianceController : ControllerBase
{
    private readonly AppliancesRepository _appliancesRepository;

    public ApplianceController()
    {
        _appliancesRepository = new AppliancesRepository();
    }

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAllAppliances()
    {
        try
        {
            var data = await _appliancesRepository.GetAllAppliances();
            return Ok(data);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    //Create a new appliance
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateAppliance([FromBody] Appliances appliance)
    {
        try
        {
            var data = await _appliancesRepository.CreateAppliances(
                appliance.type!,
                appliance.powerUsage,
                appliance.durationUsed
            );
            return Ok(data);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    //Update an appliance
    [HttpPatch]
    [Route("update")]
    public async Task<IActionResult> UpdateAppliance([FromBody] Appliances appliance)
    {
        try
        {
            var data = await _appliancesRepository.UpdateAppliances(
                appliance.applianceId,
                appliance.type!,
                appliance.powerUsage,
                appliance.durationUsed
            );
            return Ok(data);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    //Delete an appliance
    [HttpDelete]
    [Route("delete")]
    public async Task<IActionResult> DeleteAppliance([FromBody] Appliances appliance)
    {
        try
        {
            var data = await _appliancesRepository.DeleteAppliances(appliance.applianceId);
            if (data == false)
            {
                return StatusCode(
                    404,
                    "Appliance with id: " + appliance.applianceId + " does not exist"
                );
            }
            return Ok("Deleted appliance with id: " + appliance.applianceId + "");
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    //Get an appliance by id
    [HttpGet]
    [Route("get/{id}")]
    public async Task<IActionResult> GetApplianceById([FromRoute] int id)
    {
        try
        {
            var data = await _appliancesRepository.GetApplianceById(id);
            return Ok(data);
        }
        catch (Exception e)
        {
            return StatusCode(404, e.Message);
        }
    }
}
