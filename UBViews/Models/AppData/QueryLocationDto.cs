﻿namespace UBViews.Models.AppData;
public class QueryLocationDto
{
    public string Id { get; set; }
    public string Pid { get; set; }
    public List<TermOccurrenceDto> TermOccurrences { get; set; } = new();
}
