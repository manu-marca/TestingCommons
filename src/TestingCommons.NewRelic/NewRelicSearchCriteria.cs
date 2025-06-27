namespace TestingCommons.NewRelic
{
    public class NewRelicSearchCriteria
    {
        public string[] ResultColumns { get; set; }
        public string[] SearchParameters { get; set;}
        public DateTime FromTime { get; set;}
        public DateTime ToTime { get; set;}

    }
}
