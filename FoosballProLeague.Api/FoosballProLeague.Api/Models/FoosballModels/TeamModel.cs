    namespace FoosballProLeague.Api.Models.FoosballModels
{
    public class TeamModel
    {
        public int Id { get; set; }
        public List<UserModel> teamRed { get; set; }
        public List<UserModel> teamBlue { get; set; }
    }
}
