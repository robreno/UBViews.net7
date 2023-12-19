namespace UBViews.Models.Notes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models.Ubml;
public class NoteLocationsDto
{
    public List<NoteEntry> NoteLocations { get; set; } = new();
    public int Count()
    {
        return NoteLocations.Count();
    }
}
