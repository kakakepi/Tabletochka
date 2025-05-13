namespace T4bJl3T04K4
{
    public partial class Tabletochka
    {
        public class ProfileData : BaseProfileAction
        {
            public Guid Id { get; set; }
            public string Username { get; set; }
            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public string Gender { get; set; }
            public string Birthdate { get; set; }
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }
    }
}
