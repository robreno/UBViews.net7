namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models;
using UBViews.Models.Notes;
using UBViews.Models.Ubml;

public interface INoteService
{
    Task<bool> IsDirtyAsync();
    Task<int> LastNoteIdAsync();
    Task<List<NoteEntry>> GetNotesAsync();
    Task<NoteLocationsDto> GetNoteLocationsDtoAsync();
    Task<List<PaperDto>> GetPaperDtosAsync();
    Task<PaperDto> GetPaperDtoAsync(int id);
    Task<Paragraph> GetParagraphAsync(int paperId, int seqId);
    Task<Border> CreateNoteBorderAsync(NoteEntry note);
}
