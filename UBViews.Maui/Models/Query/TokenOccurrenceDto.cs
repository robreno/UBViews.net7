﻿namespace UBViews.Models.Query;
public class TokenOccurrenceDto
{
    public int PostingId { get; set; }
    public int DocumentId { get; set; }
    public int SequenceId { get; set; }
    public int SectionId { get; set; }
    public int DocumentPosition { get; set; }
    public int TextPosition { get; set; }
}
