using System.Net;
using Microsoft.AspNetCore.Mvc;
using Api.Repository;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SystemController : ControllerBase
{
  private readonly SystemsRepository _systemsRepository;

  public SystemController()
  {
    _systemsRepository = new SystemsRepository();
  }

  [HttpGet]
  [Route("all")]
  public async Task<IActionResult> GetAllSystems()
  {
    try
    {
      var data = await _systemsRepository.GetAllSystems();
      return Ok(data);
    }
    catch (Exception e)
    {
      return StatusCode(500, e.Message);
    }
  }

  //Create a new system
  [HttpPost]
  [Route("create")]
  public async Task<IActionResult> CreateSystem([FromBody] Systems system)
  {
    try
    {
      var data = await _systemsRepository.CreateSystems(
          system.inverterOutput,
          system.numberOfPanels,
          system.batterySize,
          system.numberOfBatteries,
          system.solarInput
      );

      return Ok(data);
    }
    catch (Exception e)
    {
      return StatusCode(500, e.Message);
    }
  }

  //update a system
  [HttpPatch]
  [Route("update")]
  public async Task<IActionResult> UpdateSystem([FromBody] Systems system)
  {
    try
    {
      var data = await _systemsRepository.UpdateSystems(
          system.systemId,
          system.inverterOutput,
          system.numberOfPanels,
          system.batterySize,
          system.numberOfBatteries,
          system.solarInput
      );
      return Ok(data);
    }
    catch (Exception e)
    {
      return StatusCode(500, e.Message);
    }
  }

  //delete a system
  [HttpDelete]
  [Route("delete")]
  public async Task<IActionResult> DeleteSystem([FromBody] Systems system)
  {
    try
    {
      var data = await _systemsRepository.DeleteSystems(system.systemId);
      if (data == false)
      {
        return StatusCode(404, "s with id: " + system.systemId + " not found");
      }
      return Ok("Deleted system with id: " + system.systemId + " successfully");
    }
    catch (Exception e)
    {
      return StatusCode(500, e.Message);
    }
  }

  //get a system by id
  [HttpGet]
  [Route("get/{systemId}")]
  public async Task<IActionResult> GetSystem([FromRoute] int systemId)
  {
    try
    {
      var data = await _systemsRepository.GetSystemById(systemId);
      if (data == null)
      {
        return StatusCode(404, "System with id: " + systemId + " not found");
      }
      return Ok(data);
    }
    catch (Exception e)
    {
      return StatusCode(500, e.Message);
    }
  }
}
