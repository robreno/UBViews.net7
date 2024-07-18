namespace UBViews.Models.Query;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QueryType
{
    public string BaseQuery { get; set; } = null;
    public string FilterByOp { get; set; } = null;
    public string FilterByPostfixOp { get; set; } = null;
    public string QueryString { get; set; } = null;
    public string ReverseQuery { get; set; } = null;
    public string Type { get; set; } = null;
    public int Length { get; set; } = 0;
}
