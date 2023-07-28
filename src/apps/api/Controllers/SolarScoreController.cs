using System.Net;
using Microsoft.AspNetCore.Mvc;
using Api.Repository;

//using IronPython.Hosting;
// using Microsoft.Scripting.Hosting;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SolarScoreController : ControllerBase
{
    private readonly SolarScoreRepository _solarScoreRepository;

    public SolarScoreController()
    {
        _solarScoreRepository = new SolarScoreRepository();
    }

    [HttpGet]
    [Route("mapboxkey")]
    public async Task<IActionResult> GetMapBoxKey()
    {
        try
        {
            var key = await _solarScoreRepository.GetMapBoxApiKey();
            return Ok(key);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpGet]
    [Route("getsolarscore")]
    public async Task<IActionResult> GetSolarScore([FromBody] Coordinates cord)
    {
        try
        {
            var score = await _solarScoreRepository.GetSolarScore(cord);
            return Ok(score);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }
}
