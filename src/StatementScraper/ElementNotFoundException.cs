using System;
using System.Runtime.Serialization;

namespace StatementScraper;

[Serializable]
public class ElementNotFoundException : Exception
{
    public string Selector { get; set; }

    public ElementNotFoundException(string selector, string message, Exception inner) : base(message, inner)
    {
        Selector = selector;
    }

    protected ElementNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        Selector = string.Empty;
    }
}