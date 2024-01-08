namespace UBViews.Models.Settings;

public class Setting
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
    public bool EqualTo(Setting dto)
    {
        bool isEqual = false;
        if (dto == null) { return false; }
        else
        {
            if (dto.Id == this.Id &&
                dto.Type == this.Type &&
                dto.Name == this.Name &&
                dto.Value == this.Value)
            {
                isEqual = true;
            }
        }
        return isEqual;
    }
}
