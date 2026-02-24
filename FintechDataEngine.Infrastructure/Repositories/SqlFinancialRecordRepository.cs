using System.Data;
using Microsoft.Data.SqlClient;
using FintechDataEngine.Application.Interfaces;
using FintechDataEngine.Domain.Entities;

namespace FintechDataEngine.Infrastructure.Repositories;

public class SqlFinancialRecordRepository : IFinancialRecordRepository
{
    private readonly string _connectionString;

    public SqlFinancialRecordRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task BulkInsertAsync(IEnumerable<FinancialRecord> records)
    {
        var recordList = records.ToList();
        if (!recordList.Any()) return;

        // 1. Transformación: De Objetos a DataTable (Formato tabular en memoria)
        var table = new DataTable();
        table.Columns.Add("Id", typeof(Guid));
        table.Columns.Add("TransactionName", typeof(string));
        table.Columns.Add("TransactionDate", typeof(DateTime));
        table.Columns.Add("Amount", typeof(decimal)); // El tipo decimal exacto
        table.Columns.Add("DocumentSource", typeof(string));

        foreach (var record in recordList)
        {
            table.Rows.Add(record.Id, record.TransactionName, record.TransactionDate, record.Amount, record.DocumentSource);
        }

        // 2. Carga: Inserción masiva en SQL Server
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        // SqlBulkCopy exige control absoluto sobre la escritura rápida
        using var bulkCopy = new SqlBulkCopy(connection);
        
        // Debe coincidir exactamente con el nombre de la tabla que crearemos en SQL
        bulkCopy.DestinationTableName = "FinancialRecords"; 

        // 3. ¡Boom! Escribe miles de filas en milisegundos
        await bulkCopy.WriteToServerAsync(table);
    }
}