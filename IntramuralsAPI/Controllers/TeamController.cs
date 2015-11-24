using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntramuralsAPI.Controllers
{

    [EnableCors(origins: "http://intramuraltest.azurewebsites.net", headers: "*", methods: "*")]
    public class TeamController : ApiController
    {

        // establish connection to MySQL server
        string myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=Intramurals;" +
            "Uid=bca5c68bb20976;Pwd=fa8ba175;";

        // GET: api/Team
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/players/Detroit Pistons
        [HttpGet]
        [ActionName("players")]
        public string Get(string name)
        {
            dynamic players = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;

            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            string sql = "SELECT User.usrname FROM User, UserTeam, Team WHERE User.ID = UserTeam.UsrID " +
                         "AND Team.ID = UserTeam.TeamID AND Team.name = '" + name + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                players.Add(new JObject(
                    new JProperty("name", rdr[0])));
            }
            rdr.Close();
            conn.Close();

            output = JsonConvert.SerializeObject(players);
            return output;
        }

        [HttpGet]
        [ActionName("schedule")]
        public string GetSchedule(string name)
        {
            dynamic schedule = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;

            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            string sql = "SELECT Game.ID, Game.team1ID, Game.team2ID, Game.date, Game.score1, Game.score2, Sport.name " +
                "FROM Team, Game, Sport " +
                "WHERE Team.name = '" + name + "' " +
                "AND Team.ID IN (Game.team1ID, Game.team2ID)" +
                "AND Team.sportID = Sport.ID";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                schedule.Add(new JObject(
                    new JProperty("ID", rdr[0]),
                    new JProperty("home", rdr[1]),
                    new JProperty("away", rdr[2]),
                    new JProperty("date", rdr[3]),
                    new JProperty("home_score", rdr[4]),
                    new JProperty("away_score", rdr[5]),
                    new JProperty("sport", rdr[6]))
                    );
            }
            rdr.Close();

            // add team names instead of simply team ID's
            foreach (var game in schedule)
            {
                sql = "SELECT Team.name FROM Team WHERE Team.ID = " + game["home"];
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
                rdr.Read();
                game["home"] = rdr[0].ToString();
                rdr.Close();

                sql = "SELECT Team.name FROM Team WHERE Team.ID = " + game["away"];
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
                rdr.Read();
                game["away"] = rdr[0].ToString();
                rdr.Close();
            }

            conn.Close();
            output = JsonConvert.SerializeObject(schedule);
            return output;
        }

        /*// POST: api/Team
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Team/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Team/5
        public void Delete(int id)
        {
        }*/
    }
}
