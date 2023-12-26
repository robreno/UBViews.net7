namespace UBViews.Models.Notes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NoteEntry
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int PaperId { get; set; }
    public int SequenceId { set; get; }
    public string LocationId { get; set; }
    public string Pid { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateEdited { get; set; }
    public string Author { get; set; }
    public string Subject { get; set; }
    public string Text { get; set; }
    public List<NoteText> NoteEntries { get; set; } = new();
}
