namespace WorkerService;

public class TransactionEtlOptions
{
    public string UnprocessedStatementDirectory { get; set; }
    public bool MoveProcessedStatements { get; set; }
    public string ProcessedStatementDirectory { get; set; }
}