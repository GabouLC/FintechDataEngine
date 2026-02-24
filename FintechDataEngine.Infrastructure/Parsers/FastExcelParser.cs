using ExcelDataReader;
using FintechDataEngine.Application.Interfaces;
using FintechDataEngine.Domain.Entities;
using System.Globalization;

namespace FintechDataEngine.Infrastructure.Parsers;

public class FastExcelParser : IExcelParser
{
    public FastExcelParser()
    {
        // Requisito obligatorio de .NET 8 para que ExcelDataReader pueda leer la codificación de los archivos
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    public async Task<List<(FinancialRecord? Record, string? Error)>> ParseAsync(Stream fileStream, string fileName)
    {
        var results = new List<(FinancialRecord? Record, string? Error)>();

        // Envolvemos la lectura síncrona en un Task para no bloquear el hilo de la API
        await Task.Run(() =>
        {
            using var reader = ExcelReaderFactory.CreateReader(fileStream);
            
            // Saltamos la primera fila asumiendo que son los encabezados (Nombre, Fecha, Monto)
            reader.Read(); 

            int rowNumber = 2; // Empezamos en la fila 2 para los mensajes de error
            
            while (reader.Read())
            {
                try
                {
                    // 1. Nombre (Lo pasamos a string sin problema)
                    string? name = reader.GetValue(0)?.ToString();
                    
                    // 2. Fecha Segura (Soporta que Excel lo mande como fecha real o como texto)
                    DateTime date;
                    var rawDate = reader.GetValue(1);
                    if (rawDate is DateTime dt) {
                        date = dt;
                    } else {
                        date = Convert.ToDateTime(rawDate);
                    }

                    // 3. Monto Seguro (A prueba de culturas e idiomas)
                    var rawAmount = reader.GetValue(2);
                    decimal amount = 0;
                    if (rawAmount is double d) {
                        amount = Convert.ToDecimal(d);
                    } else {
                        // Forzamos el estándar internacional (punto como decimal)
                        amount = Convert.ToDecimal(rawAmount, CultureInfo.InvariantCulture);
                    }

                    // Llamamos a las reglas de tu Dominio
                    var (record, error) = FinancialRecord.Create(name ?? "", date, amount, fileName);
                    
                    results.Add((record, error));
                }
                catch (Exception ex)
                {
                    // Si la celda de fecha tiene texto, o el número es inválido, entra aquí.
                    // ¡El sistema no se cae! Solo guardamos el error y pasamos a la siguiente fila.
                    results.Add((null, $"Fila {rowNumber}: Formato de celda inválido. Detalle: {ex.Message}"));
                }
                
                rowNumber++;
            }
        });

        return results;
    }
}