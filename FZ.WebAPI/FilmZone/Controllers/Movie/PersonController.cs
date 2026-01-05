using FZ.Movie.ApplicationService.Service.Abtracts;
using FZ.Movie.Dtos.Request;
using Microsoft.AspNetCore.Mvc;

namespace FZ.WebAPI.Controllers.Movie
{
    [Route("movie/[controller]/[action]")]
    [ApiController]
    public class PersonController : Controller
    {
        private readonly IPersonService _personService;
        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }
        [HttpPost]
        public async Task<IActionResult> CreatePerson([FromForm] CreatePersonRequest createPersonRequest , CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _personService.CreatePerson(createPersonRequest ,ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdatePerson([FromForm] UpdatePersonRequest updatePersonRequest , CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _personService.UpdatePerson(updatePersonRequest ,ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerson(int id, CancellationToken ct)
        {
            try
            {
                var result = await _personService.DeletePerson(id, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("{ID}")]
        public async Task<IActionResult> GetPersonByID(int ID, CancellationToken ct)
        {
            try
            {
                var result = await _personService.GetPersonByID(ID, ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }
        [HttpGet("getall")]
        public async Task<IActionResult> GetAllPerson( CancellationToken ct)
        {
            try
            {
                var result = await _personService.GetPeople( ct);
                if (result.ErrorCode != 200)
                {
                    BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }



    }
}
