/*******************************************************************
 *  LeagueController.cs
 *
 *  Creates all API calls concerning a league.
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
    // Allow cross-origin requests from our client, and test clients 
    [EnableCors(origins: "http://intramuraltest.azurewebsites.net,http://manji.azurewebsites.net,http://localhost:8080", headers: "*", methods: "*")]
    public class LeagueController : ApiController
    {

        // establish connection to MySQL server
        string myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=Intramurals;" +
            "Uid=bca5c68bb20976;Pwd=fa8ba175;";

        /* 
         *  GET: api/league/leagues/late fall
         *  Retrieves a list of leagues for the given season
         *  @param: string name, specified season name.
         *  @return: a list of leagues for the season in JSON format.
         */
        [HttpGet]
        [ActionName("leagues")]
        public string GetLeagues(string name)
        {
            // initiate JArray for JSON data
            dynamic schedule = new JArray();
            string output = "";

            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query command
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
                // add league name to JArray
                schedule.Add(new JObject(
                    new JProperty("name", rdr[0]))
                    );
            }
            rdr.Close();
            conn.Close();

            // serialize JSON data
            output = JsonConvert.SerializeObject(schedule);
            return output;
        }

        /* 
         *  GET: api/league/teams/Basketball
         *  Retrieves a list of teams for the given league with wins and losses.
         *  @param: string name, specified league name.
         *  @return: a list of teams in the league with wins and losses in JSON format.
         */
        [HttpGet]
        [ActionName("teams")]
        public string GetTeams(string name)
        {
            // initiate JArray for JSON data
            dynamic teams = new JArray();
            string output = "";

            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query command
            string sql = "SELECT Team.name " +
                "FROM Team, Sport " +
                "WHERE Sport.name = '" + name + "' " +
                "AND Team.sportID = Sport.ID ";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                // add data to JArray
                // NOTE: wins and losses have 0's for place holders currently
                teams.Add(new JObject(
                    new JProperty("name", rdr[0]),
                    new JProperty("wins", 0),
                    new JProperty("losses", 0))                 
                    );
            }
            rdr.Close();

            // calculate the wins and losses for each team in leauge
            foreach (var team in teams)
            {
                int wins = 0;
                int losses = 0;

                // find all wins for team when team was home (team1ID in database)
                sql = "SELECT Game.score1, Game.score2 " +
                    "FROM Game, Team " +
                    "WHERE Team.name = '" + team["name"] + "' " +
                    "AND Team.ID = Game.team1ID " + 
                    "AND Game.date < DATE(NOW())";
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();

                // loops through each game
                while (rdr.Read())
                {
                    // make sure both home and away score are valid - not null
                    if (! DBNull.Value.Equals(rdr[0]) && ! DBNull.Value.Equals(rdr[1]))
                    {
                        // if home score is greater, add to wins
                        if (Convert.ToInt16(rdr[0]) > Convert.ToInt16(rdr[1]))
                            wins++;

                        // if away score is greater, add to losses
                        else
                            losses++;
                    }
                }
                rdr.Close();

                // find all wins and losses for when team was away (team2ID in database)
                sql = "SELECT Game.score1, Game.score2 " +
                    "FROM Game, Team " +
                    "WHERE Team.name = '" + team["name"] + "' " +
                    "AND Team.ID = Game.team2ID " +
                    "AND Game.date < DATE(NOW())";
                cmd = new MySqlCommand(sql, conn);
                rdr = cmd.ExecuteReader();

                // loops through each game
                while (rdr.Read())
                {
                    // make sure both home and away score are valid - not null
                    if (!DBNull.Value.Equals(rdr[0]) && !DBNull.Value.Equals(rdr[1]))
                    {
                        // if away score is greater, add to wins
                        if (Convert.ToInt16(rdr[1]) > Convert.ToInt16(rdr[0]))
                            wins++;

                        // if home score is greater, add to losses
                        else
                            losses++;
                    }
                }

                rdr.Close();

                // update wins and losses in JArray
                team["wins"] = wins;
                team["losses"] = losses;
            }

            conn.Close();

            // serialize JSON data
            output = JsonConvert.SerializeObject(teams);
            return output;
        }

        /* 
         *  GET: api/league/schedule/Basketball
         *  Retrieves the schedule for the given league.
         *  @param: string name, specified league name.
         *  @return: a list of games (schedule) for the given league.
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
            string sql = "SELECT Game.ID, Game.team1ID, Game.team2ID, UNIX_TIMESTAMP(Game.date) * 1000, Game.score1, Game.score2 " +
                "FROM Team, Game, Sport " +
                "WHERE Sport.name = '" + name + "' " +
                "AND Team.sportID = Sport.ID " +
                "AND Team.ID IN (Game.team1ID, Game.team2ID)";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                // add data to JArray
                // home and aways teams are given as ID #'s
                schedule.Add(new JObject(
                    new JProperty("ID", rdr[0]),
                    new JProperty("home", rdr[1]),
                    new JProperty("away", rdr[2]),
                    new JProperty("date", rdr[3]),
                    new JProperty("home_score", rdr[4]),
                    new JProperty("away_score", rdr[5]))
                    );

                // for some reason, games were duplicated in this query, so to compensate,
                // this read skips every other game for output
                rdr.Read();
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
