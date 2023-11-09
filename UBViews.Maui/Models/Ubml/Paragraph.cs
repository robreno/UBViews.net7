namespace UBViews.Models.Ubml;
public class Paragraph
{
    private string endTime;
    public int PaperId { get; set; }
    public int SeqId { get; set; }
    public string PaperIdSeqId { get; set; }
    public string Uid { get; set; }
    public string Pid { get; set; }
    public string Type { get; set; }
    public string ParaStyle { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get => endTime; set => endTime = value; }
    public string Text { get; set; }
    public List<Run> Runs { get; set; } = new List<Run>();
}
