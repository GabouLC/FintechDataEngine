using FintechDataEngine.Domain.Entities;

namespace FintechDataEngine.Application.Interfaces;

public interface IFinancialRecordRepository
{
    // Usamos Bulk Insert por rendimiento. Guardar 1x1 mataría la API.
    Task BulkInsertAsync(IEnumerable<FinancialRecord> records);
}