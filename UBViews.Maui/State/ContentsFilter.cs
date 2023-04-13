using System.Collections.ObjectModel;
using UBViews.Models;

namespace UBViews.State;
public partial class ContentsFilter
{
    public ObservableCollection<ContentDto> ContentDtos { get; set; }

    // CStor
    public ContentsFilter() { }

    public async Task Create()
    {
        await Task.Run(() =>
        {
            ContentDtos = new ObservableCollection<ContentDto>()
            {
                new ContentDto { PaperId = 0, PaperTitle = "Foreword", ContentTitles = new List<TitleDto> () 
                    { 
                        new TitleDto() { Prefix = "I", SectionTitle = "Deity and  Divinity" },
                        new TitleDto() { Prefix = "II", SectionTitle = "God" },
                        new TitleDto() { Prefix = "III", SectionTitle = "The First Source and Center" },
                        new TitleDto() { Prefix = "IV", SectionTitle = "Universal Reality" },
                        new TitleDto() { Prefix = "V", SectionTitle = "Personality Realities" },
                        //"VI. Energy and Pattern", "VII. The Supreme Being", "VIII. God the Sevenfold", "IX. God the Ultimate", "X. God the Absolute", "XI. The Three Absolutes", "XII. The Trinities" 
                    }
                },
            };
        });
    }
}

