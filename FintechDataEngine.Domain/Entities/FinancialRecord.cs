namespace FintechDataEngine.Domain.Entities;

public class FinancialRecord
{
    public Guid Id { get; private set; }
    public string TransactionName { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public decimal Amount { get; private set; } // ¡Decimal, como acordamos!
    public string DocumentSource { get; private set; }

    // Constructor privado para forzar el uso de un método de creación o factory
    private FinancialRecord(string name, DateTime date, decimal amount, string source)
    {
        Id = Guid.NewGuid();
        TransactionName = name;
        TransactionDate = date;
        Amount = amount;
        DocumentSource = source;
    }

    // Método Factory con Validación de Negocio básica
    public static (FinancialRecord? Record, string? Error) Create(string name, DateTime date, decimal amount, string source)
    {
        if (amount <= 0) return (null, "El monto debe ser mayor a cero.");
        if (string.IsNullOrWhiteSpace(name)) return (null, "El nombre del registro es obligatorio.");
        if (date > DateTime.UtcNow) return (null, "La fecha no puede ser futura.");

        return (new FinancialRecord(name, date, amount, source), null);
    }
}