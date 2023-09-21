using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBViews.Models.Query;

public class TokenOccurrenceComparer
{
    public int DocumentId { get; set; }
    public int SequenceId { get; set; }
    public int DocumentPosition { get; set; }
    public int TextPosition { get; set; }
    public int TextLength { get; set; }
    public string Term { get; set; }
    public string ParagraphId { get; set; }
}
