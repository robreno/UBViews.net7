namespace UBViews.Models.XmlAppData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class TermOccurrenceDto
{
    public string Term { get; set; }
    public int DocId { get; set; }
    public int SeqId { get; set; }
    public int DpoId { get; set; }
    public int TpoId { get; set; }
    public int Len { get; set; }
}
