﻿using SQLite;

namespace UBViews.Repositories.Models;
[Table("TokenOccurrences")]
public class TokenOccurrence
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int PostingId { get; set; }
    public int DocumentId { get; set; }
    public int SequenceId { get; set; }
    public int DocummentPosition { get; set; }
    public int TextPosition { get; set; }
}