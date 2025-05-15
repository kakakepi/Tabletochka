namespace T4bJl3T04K4
{
    public partial class Tabletochka
    {
        public class DiagnoseRequest
        {
            public string action { get; set; }
            public List<Guid> SelectedSymptomIds { get; set; }
        }

    }
}
