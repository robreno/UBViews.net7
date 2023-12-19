namespace UBViews.Models.Notes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

public class ParagraphNote
{
    public ParagraphNote()
    {

    }

    public ParagraphNote(int id, NoteEntry noteEntry)
    {
        this.Id = id;
        this.Notes.Add(noteEntry);
    }

    public int Id { get; set; }
    public string UniqueId { get; set; }
    public List<NoteEntry> Notes { get; set; } = new();
    public List<Span> Spans { get; set; }
}
