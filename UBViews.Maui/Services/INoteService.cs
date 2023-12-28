namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models.Notes;

public interface INoteService
{
    Task<NoteLocationsDto> GetNoteLocationsDtoAsync();
    Task<Border> CreateNoteBorderAsync(NoteEntry note);
    Task<List<NoteEntry>> LoadNotesAsync();
}
