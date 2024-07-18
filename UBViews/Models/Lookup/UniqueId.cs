namespace UBViews.Models;

public class UniqueId
{
    public UniqueId()
    {
        Id = 0;
        Pid = 0 + ":" + 0 + "." + 0;
        PaperId = "0";
        SectionId = "0";
        ParagraphId = "0";
        SequenceId = "1";
        LocationId = PaperId + "." + SequenceId;
    }
    public UniqueId(string paperId, string sectionId, string paragraphId)
    {
        Id = Int32.Parse(paperId);
        Pid = paperId + ":" + sectionId + "." + paragraphId;
        PaperId = paperId;
        SectionId = sectionId;
        ParagraphId = paragraphId;
        SequenceId = string.Empty;
        LocationId = string.Empty;
    }
    public int Id { get; set; }
    public string Pid = string.Empty;
    public string PaperId = string.Empty;
    public string SectionId = string.Empty;
    public string ParagraphId = string.Empty;
    public string SequenceId = string.Empty;
    public string LocationId { get; set; } = string.Empty;
}