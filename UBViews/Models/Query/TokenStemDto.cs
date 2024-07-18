using System;

namespace UBViews.Models.Query
{
    public class TokenStemDto
    {
        public int Id { get; set; } = 0;
        public string Lexeme { get; set; } = string.Empty;
        public string Stemmed { get; set; } = string.Empty;
    }
}
