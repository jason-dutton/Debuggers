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
      var data = await _appliancesRepository.GetAllAplliances();
      return Ok(data);
    }
    catch (Exception e)
    {
      return StatusCode(500, e.Message);
    }
  }
}
