using FintechDataEngine.Application.DTOs;
using FintechDataEngine.Application.Interfaces;
using FintechDataEngine.Domain.Entities;

namespace FintechDataEngine.Application.Services;

public class FinancialDataProcessorService
{
    private readonly IExcelParser _excelParser;
    private readonly IFinancialRecordRepository _repository;

    // Inyección de dependencias (Dependency Injection)
    public FinancialDataProcessorService(IExcelParser excelParser, IFinancialRecordRepository repository)
    {
        _excelParser = excelParser;
        _repository = repository;
    }

    public async Task<ProcessResultDto> ProcessFileAsync(Stream fileStream, string fileName)
    {
        // 1. EXTRACT & TRANSFORM: Delegamos la lectura al parser (que usará las reglas del Dominio)
        var parsedRecords = await _excelParser.ParseAsync(fileStream, fileName);

        var validRecords = new List<FinancialRecord>();
        var resultsDto = new List<RecordResultDto>();

        // 2. CLASIFICACIÓN (Tolerancia a fallos)
        for (int i = 0; i < parsedRecords.Count; i++)
        {
            var item = parsedRecords[i];
            
            if (item.Error == null && item.Record != null)
            {
                validRecords.Add(item.Record);
                resultsDto.Add(new RecordResultDto(item.Record.TransactionName, $"Monto: {item.Record.Amount}", "Success"));
            }
            else
            {
                // Si falló, guardamos el error para el reporte final sin detener el proceso
                resultsDto.Add(new RecordResultDto($"Fila {i + 2}", "N/A", "Error", item.Error));
            }
        }

        // 3. LOAD: Persistencia masiva solo de los válidos
        if (validRecords.Any())
        {
            await _repository.BulkInsertAsync(validRecords);
        }

        // 4. RESPUESTA: Construimos el JSON final para el cliente
        bool hasErrors = parsedRecords.Count != validRecords.Count;
        
        return new ProcessResultDto(
            ProcessId: $"FIN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            Status: hasErrors ? "CompletedWithErrors" : "Completed",
            Timestamp: DateTime.UtcNow,
            TotalRecordsProcessed: parsedRecords.Count,
            SuccessfulInserts: validRecords.Count,
            FailedRecords: parsedRecords.Count - validRecords.Count,
            Results: resultsDto
        );
    }
}