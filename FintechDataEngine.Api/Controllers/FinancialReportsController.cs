using Microsoft.AspNetCore.Mvc;
using FintechDataEngine.Application.Services;

namespace FintechDataEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinancialReportsController : ControllerBase
{
    private readonly FinancialDataProcessorService _processorService;

    // Inyectamos tu Orquestador
    public FinancialReportsController(FinancialDataProcessorService processorService)
    {
        _processorService = processorService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadExcel(IFormFile file)
    {
        // 1. Validaciones básicas de la petición HTTP
        if (file == null || file.Length == 0)
            return BadRequest("No se proporcionó un archivo válido.");

        var extension = Path.GetExtension(file.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
            return BadRequest("El archivo debe ser un formato Excel (.xlsx o .xls).");

        // 2. Abrimos el flujo de datos (Stream) sin guardar el archivo en el disco del servidor
        using var stream = file.OpenReadStream();
        
        // 3. ¡Magia! Le pasamos el problema a tu caso de uso
        var result = await _processorService.ProcessFileAsync(stream, file.FileName);

        // 4. Devolvemos el JSON estructurado (Status 200 OK)
        return Ok(result);
    }
}