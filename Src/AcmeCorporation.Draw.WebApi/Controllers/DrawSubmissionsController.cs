using System;
using System.Linq;
using System.Threading.Tasks;
using AcmeCorporation.Raffle.Domain;
using AcmeCorporation.Raffle.Domain.Interfaces;
using AcmeCorporation.Raffle.Infrastructure.Services;
using AcmeCorporation.Raffle.WebApi.Extensions;
using AcmeCorporation.Raffle.WebApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcmeCorporation.Raffle.WebApi.Controllers
{
    [Route("submissions")]
    public class DrawSubmissionsController : ControllerBase
    {
        private readonly IDrawSubmissionService _submissionService;
        private readonly ISerialNumberRepository _serialNumberRepository;

        public DrawSubmissionsController(IDrawSubmissionService submissionService, ISerialNumberRepository serialNumberRepository)
        {
            _submissionService = submissionService;
            _serialNumberRepository = serialNumberRepository;
        }
        
        // [Authorize] if security was enabled this wouldnt be commented out
        [HttpGet, Route("/pages/{page}")] // /pages/to avoid defacto rest getbyId convention
        public async Task<IActionResult> Get(int page)
        {
            var pagedResult = await _submissionService.GetSubmissions(page);
            var dto = pagedResult.ToDto();

            return new OkObjectResult(dto);
        }
        
        
        [HttpPost]
        public async Task<IActionResult> SubmitDraw([FromBody]SubmitDrawRequest request)
        {
            var serialNumber = await _serialNumberRepository.GetSerialNumber(request.SerialNumber);
            if (serialNumber == null)
                return BadRequest();
    
            var submission = await _submissionService.Submit(request.FirstName, request.LastName, new EmailAddress(request.EmailAddress),
                serialNumber);

            var dto = submission.ToDto(includeSerial: true);
            return new OkObjectResult(dto);
        }
    }
}