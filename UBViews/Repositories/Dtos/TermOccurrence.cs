﻿namespace UBViews.Repositories.Dtos;
public class TermOccurence
{
    public string Term { get; set; }
    public int DocId { get; set; }
    public int SeqId { get; set; }
    public int DpoId { get; set; }
    public int TpoId { get; set; }
    public int Len { get; set; }
}
