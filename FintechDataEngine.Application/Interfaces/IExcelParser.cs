using FintechDataEngine.Domain.Entities;

namespace FintechDataEngine.Application.Interfaces;

public interface IExcelParser
{
    // Recibe un archivo en crudo (Stream) y devuelve una lista de tuplas
    // Cada tupla contiene el registro válido O el mensaje de error de esa fila
    Task<List<(FinancialRecord? Record, string? Error)>> ParseAsync(Stream fileStream, string fileName);
}