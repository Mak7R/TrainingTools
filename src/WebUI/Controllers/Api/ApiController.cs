using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers.Api;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class ApiController : ControllerBase
{
    
}