using FZ.Auth.ApplicationService.Billing;
using FZ.Auth.Dtos.Billing;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Payment
{
    [ApiController]
    [Route("api/plans")]
    public class PlanController : Controller
    {
       private readonly IPlanService _plans;
         public PlanController(IPlanService plans)
         {
              _plans = plans;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreatePlan([FromBody] CreatePlanRequestDto dto, CancellationToken ct)
        {
            
             try 
             {
                  var res = await _plans.CreatePlan(dto, ct);
                  if (res.ErrorCode == 200)
                  {
                       return Ok(res);
                  }
                  return BadRequest(res);
             }
             catch (Exception ex)
             {
                  return StatusCode(500, $"Internal server error: {ex.Message}");
             }
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdatePlan([FromBody] UpdatePlanRequestDto dto, CancellationToken ct)
        {
          
             try 
             {
                  var res = await _plans.UpdatePlan(dto, ct);
                  if (res.ErrorCode == 200)
                  {
                       return Ok(res);
                  }
                  return BadRequest(res);
             }
             catch (Exception ex)
             {
                  return StatusCode(500, $"Internal server error: {ex.Message}");
             }
        }
        [HttpDelete("delete/{planID}")]
        public async Task<IActionResult> DeletePlan([FromRoute] int planID, CancellationToken ct)
        {
             if (planID <= 0)
             {
                  return BadRequest("Invalid plan ID.");
             }
             try 
             {
                  var res = await _plans.DeletePlan(planID, ct);
                  if (res.ErrorCode == 200)
                  {
                       return Ok(res);
                  }
                  return BadRequest(res);
             }
             catch (Exception ex)
             {
                  return StatusCode(500, $"Internal server error: {ex.Message}");
             }
        }
        [HttpGet("{planID}")]
        public async Task<IActionResult> GetPlanByID([FromRoute] int planID, CancellationToken ct)
        {
             if (planID <= 0)
             {
                  return BadRequest("Invalid plan ID.");
             }
             try 
             {
                  var res = await _plans.GetPlanByID(planID, ct);
                  if (res.ErrorCode == 200)
                  {
                       return Ok(res);
                  }
                  return NotFound(res);
             }
             catch (Exception ex)
             {
                  return StatusCode(500, $"Internal server error: {ex.Message}");
             }
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPlans(CancellationToken ct)
        {
             try 
             {
                  var res = await _plans.GetAllPlans();
                  if (res.ErrorCode == 200)
                  {
                       return Ok(res);
                  }
                  return NotFound(res);
             }
             catch (Exception ex)
             {
                  return StatusCode(500, $"Internal server error: {ex.Message}");
             }
        }
    }
}
