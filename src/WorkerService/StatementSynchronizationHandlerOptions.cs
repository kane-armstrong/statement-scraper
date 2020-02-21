namespace WorkerService
{
    public class StatementSynchronizationHandlerOptions
    {
        public string UnprocessedStatementDirectory { get; set; }
        public bool MoveProcessedStatements { get; set; }
        public string ProcessedStatementDirectory { get; set; }
    }
}
