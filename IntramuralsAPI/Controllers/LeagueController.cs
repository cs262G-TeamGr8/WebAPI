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

    [EnableCors(origins: "http://intramuraltest.azurewebsites.net,http://manji.azurewebsites.net", headers: "*", methods: "*")]
    public class LeagueController : ApiController
    {

        // establish connection to MySQL server
        string myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=Intramurals;" +
            "Uid=bca5c68bb20976;Pwd=fa8ba175;";


        [HttpGet]
        [ActionName("leagues")]
        public string GetLeagues(string name)
        {
            dynamic schedule = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;

            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            string sql = "SELECT Sport.name " +
                "FROM Sport, Season, SportSeason " +
                "WHERE Season.name = '" + name + "' " +
                "AND Season.ID = SportSeason.seasonID " +
                "AND Sport.ID = SportSeason.sportID";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                schedule.Add(new JObject(
                    new JProperty("name", rdr[0]))
                    );
            }
            rdr.Close();
            conn.Close();
            output = JsonConvert.SerializeObject(schedule);
            return output;
        }

        [HttpGet]
        [ActionName("teams")]
        public string GetTeams(string name)
        {
            dynamic teams = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;

            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            string sql = "SELECT Team.name " +
                "FROM Team, Sport " +
                "WHERE Sport.name = '" + name + "' " +
                "AND Team.sportID = Sport.ID ";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();


            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                teams.Add(new JObject(
                    new JProperty("name", rdr[0]),
                    new JProperty("wins", 0),
                    new JProperty("losses", 0))                 
                    );
            }
            rdr.Close();

            foreach (var team in teams)
            {
                int wins = 0;
                int losses = 0;

                sql = "SELECT Game.score1, Game.score2 " +
                    "FROM Game, Team " +
                    "WHERE Team.name = '" + team["name"] + "' " +
                    "AND Team.ID = Game.team1ID " + 
                    "AND Game.date < DATE(NOW())";
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (! DBNull.Value.Equals(rdr[0]) && ! DBNull.Value.Equals(rdr[1]))
                    {
                        if (Convert.ToInt16(rdr[0]) > Convert.ToInt16(rdr[1]))
                            wins++;
                        else
                            losses++;
                    }
                }

                rdr.Close();

                sql = "SELECT Game.score1, Game.score2 " +
                    "FROM Game, Team " +
                    "WHERE Team.name = '" + team["name"] + "' " +
                    "AND Team.ID = Game.team2ID " +
                    "AND Game.date < DATE(NOW())";
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    if (!DBNull.Value.Equals(rdr[0]) && !DBNull.Value.Equals(rdr[1]))
                    {
                        if (Convert.ToInt16(rdr[1]) > Convert.ToInt16(rdr[0]))
                            wins++;
                        else
                            losses++;
                    }
                }

                rdr.Close();

                team["wins"] = wins;
                team["losses"] = losses;
            }


            conn.Close();

            output = JsonConvert.SerializeObject(teams);
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

            string sql = "SELECT Game.ID, Game.team1ID, Game.team2ID, Game.date, Game.score1, Game.score2 " +
                "FROM Team, Game, Sport " +
                "WHERE Sport.name = '" + name + "' " +
                "AND Team.sportID = Sport.ID " +
                "AND Team.ID IN (Game.team1ID, Game.team2ID)";
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
                    new JProperty("away_score", rdr[5]))
                    );
                rdr.Read();
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

        /*// POST: api/League
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/League/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/League/5
        public void Delete(int id)
        {
        }*/
    }
}
