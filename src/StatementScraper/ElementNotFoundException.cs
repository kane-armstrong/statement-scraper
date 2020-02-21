using System;

namespace StatementScraper
{
    public class ElementNotFoundException : Exception
    {
        public const string Hint = "The website may have changed the name, id, or class(es) of the element you are referencing. Please check the element on the website.";

        public string Selector { get; set; }

        public ElementNotFoundException(string selector)
        {
            Selector = selector;
        }

        public ElementNotFoundException(string selector, string message) : base(message)
        {
            Selector = selector;
        }

        public ElementNotFoundException(string selector, string message, Exception inner) : base(message, inner)
        {
            Selector = selector;
        }
    }
}
