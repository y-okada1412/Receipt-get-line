public class ReceiptAnalysis
{
    public int Status { get; set; }
    public string? CreatedDateTime { get; set; }
    public string? LastUpdatedDateTime { get; set; }
    public AnalyzeResult? AnalyzeResult { get; set; }
}

public class AnalyzeResult
{
    public string? Version { get; set; }
    public string? ModelVersion { get; set; }
    public List<ReadResult>? ReadResults { get; set; }
}

public class ReadResult
{
    public int Page { get; set; }
    public string? Language { get; set; }
    public double Angle { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int Unit { get; set; }
    public List<Line>? Lines { get; set; }
}

public class Line
{
    public string? Language { get; set; }
    public List<int>? BoundingBox { get; set; }
    public Appearance? Appearance { get; set; }
    public string? Text { get; set; }
    public List<Word>? Words { get; set; }
}

public class Appearance
{
    public Style? Style { get; set; }
}

public class Style
{
    public string? Name { get; set; }
    public double Confidence { get; set; }
}

public class Word
{
    public List<int>? BoundingBox { get; set; }
    public string? Text { get; set; }
    public double Confidence { get; set; }
}
//To rotate class
public class boxDetail
{
    public int lineNum { get; set;}
    public double bias { get; set; }
    public double width { get; set; }

}