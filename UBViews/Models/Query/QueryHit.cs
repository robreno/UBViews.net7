namespace UBViews.Models.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models.Ubml;

public class QueryHit
{
    public string Id { get; set; }
    public string Query { get; set; }
    public int Hit { get; set; } = 0;
    public int PaperId { get; set; } = 0;
    public int SequenceId { get; set; } = 0;
    public string Pid { get; set; }
    public bool Selected { get; set; }
    public Paragraph Paragraph { get; set; }
}
