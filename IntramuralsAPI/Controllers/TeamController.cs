/*******************************************************************
 *  TeamController.cs
 *
 *  Creates all API calls concerning a team.
 *
 ******************************************************************/

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
    /* 
     *  Team class defines an team used to create a new team.
     *  Contains League, Name, Email.
     */
    public class Team
    {
        public string League { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    // Allow cross-origin requests from our client, and test clients 
    [EnableCors(origins: "http://intramuraltest.azurewebsites.net,http://manji.azurewebsites.net,http://localhost:8080", headers: "*", methods: "*")]
    public class TeamController : ApiController
    {

        // establish connection to MySQL server
        string myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=Intramurals;" +
            "Uid=bca5c68bb20976;Pwd=fa8ba175;";

        /* 
         *  GET: api/team/players/Yellow Vitamin Water
         *  Retrieves the players on the specified team.
         *  @param: string name, specified team name.
         *  @return: a list of players on the team in JSON format.
         */
        [HttpGet]
        [ActionName("players")]
        public string Get(string name)
        {
            // initiate JArray for JSON data
            dynamic players = new JArray();
            string output = "";

            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query command
            string sql = "SELECT User.usrname FROM User, UserTeam, Team WHERE User.ID = UserTeam.UsrID " +
                         "AND Team.ID = UserTeam.TeamID AND Team.name = '" + name + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                // add name to JArray
                players.Add(new JObject(
                    new JProperty("name", rdr[0])));
            }
            rdr.Close();
            conn.Close();

            // serialize JSON data
            output = JsonConvert.SerializeObject(players);
            return output;
        }

        /* 
         *  GET: api/team/schedule/Yellow Vitamin Water
         *  Retrieves the schedule for the specified team.
         *  @param: string name, specified team name.
         *  @return: a list of games (schedule) for the team in JSON format.
         */
        [HttpGet]
        [ActionName("schedule")]
        public string GetSchedule(string name)
        {
            // initiate JArray for JSON data
            dynamic schedule = new JArray();
            string output = "";

            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query command
            string sql = "SELECT Game.ID, Game.team1ID, Game.team2ID, UNIX_TIMESTAMP(Game.date) * 1000, Game.score1, Game.score2, Sport.name " +
                "FROM Team, Game, Sport " +
                "WHERE Team.name = '" + name + "' " +
                "AND Team.ID IN (Game.team1ID, Game.team2ID)" +
                "AND Team.sportID = Sport.ID";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                // add each game to JArray
                // home and aways teams are given as ID #'s
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
                // sql query command for home team
                sql = "SELECT Team.name FROM Team WHERE Team.ID = " + game["home"];
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
                rdr.Read();
                // change ID to team name
                game["home"] = rdr[0].ToString();
                rdr.Close();

                // sql query command for away team
                sql = "SELECT Team.name FROM Team WHERE Team.ID = " + game["away"];
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();
                rdr.Read();
                // change ID to team name
                game["away"] = rdr[0].ToString();
                rdr.Close();
            }
            conn.Close();

            // serialize JSON data
            output = JsonConvert.SerializeObject(schedule);
            return output;
        }

        /* 
         *  POST: api/team/new
         *  Creates a new team in the database.
         *  @param: Team tm, an instance of Team class with new team information.
         *  @return: string tm.Name, the name of the team just created.
         */
        [Route("api/team/new")]
        [HttpPost]
        public string NewTeam(Team tm)
        {
            // store today's date for the start date of the new team
            DateTime today = DateTime.Now;
            string startDate = today.ToString("yyyy-MM-dd");

            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // this sql command finds the team league ID # associated with the team name
            string sql = "SELECT ID FROM Sport WHERE Sport.name = '" + tm.League + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            // sportID now stores the league ID # associated with the new team
            int sportID = Int16.Parse(rdr[0].ToString());
            rdr.Close();

            // create new sql command
            cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Team (startDate, sportID, name, contact) VALUES (@startDate, @id, @name, @email)";

            // add values to parameters in sql command
            cmd.Parameters.AddWithValue("@startDate", startDate);
            cmd.Parameters.AddWithValue("@id", sportID);
            cmd.Parameters.AddWithValue("@name", tm.Name);
            cmd.Parameters.AddWithValue("@email", tm.Email);
            cmd.ExecuteNonQuery();

            return tm.Name;
        }

        /*// PUT: api/Team/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Team/5
        public void Delete(int id)
        {
        }*/
    }
}
