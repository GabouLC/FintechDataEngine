namespace FintechDataEngine.Application.DTOs;

public record ProcessResultDto(
    string ProcessId,
    string Status,
    DateTime Timestamp,
    int TotalRecordsProcessed,
    int SuccessfulInserts,
    int FailedRecords,
    List<RecordResultDto> Results
);

public record RecordResultDto(
    string RecordName, 
    string KeyData, 
    string Status, 
    string? ErrorMessage = null
);