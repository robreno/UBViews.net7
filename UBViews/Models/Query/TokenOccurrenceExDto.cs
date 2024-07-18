using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBViews.Models.Query;

public class TokenOccurrenceExDto
{
    public int Id { get; set; }
    public int PostingId { get; set; }
    public int DocumentId { get; set; }
    public int SequenceId { get; set; }
    public int SectionId { get; set; }
    public int DocumentPosition { get; set; }
    public int TextPosition { get; set; }
    public string ParagraphId { get; set; }
}
