using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Web.Http.Cors;

namespace IntramuralsAPI.Controllers
{
    [EnableCors(origins: "http://intramuraltest.azurewebsites.net", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        // establish connection to MySQL server
        string myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=IntraTest;" +
            "Uid=bbd3fdf9969899;Pwd=7c348d21;";

        /* GET: api/user/info/Michael Jordan
           Retrieves the relevant infomation regarding the specfied user.
           Returns the ID, name, and email in JSON format
           */
        [HttpGet]
        [ActionName("info")]
        public string Get(string name)
        {
            dynamic info = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;

            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query
            string sql = "SELECT * FROM User WHERE User.usrname = '" + name + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                info.Add(new JObject(
                    new JProperty("ID", rdr[0]),
                    new JProperty("name", rdr[1]),
                    new JProperty("email", rdr[3]))
                    );
            }
            rdr.Close();
            conn.Close();

            output = JsonConvert.SerializeObject(info);
            return output;
        }

        [Route("api/user/login")]
        [HttpGet]
        public string CheckLogin(string email, string password)
        {
            dynamic info = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;

            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query
            string sql = "SELECT User.pw, User.email, User.usrname, User.ID FROM User WHERE User.email = '" + email + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            rdr.Read();

            if (rdr.HasRows)
            {
                if (rdr[0].ToString() == password)
                {
                    info.Add(new JObject(
                        new JProperty("loggedIn", true),
                        new JProperty("email", rdr[1]),
                        new JProperty("name", rdr[2]),
                        new JProperty("id", rdr[3]),
                        new JProperty("message", "Login successful"))
                        );
                }
                else
                {
                    info.Add(new JObject(
                        new JProperty("loggedIn", false),
                        new JProperty("email", rdr[1]),
                        new JProperty("name", null),
                        new JProperty("id", null),
                        new JProperty("message", "Password is incorrect"))
                        );
                }
            }
            else
            {
                info.Add(new JObject(
                    new JProperty("loggedIn", false),
                    new JProperty("email", null),
                    new JProperty("name", null),
                    new JProperty("id", null),
                    new JProperty("message", "Email not found"))
                    );
            }

            rdr.Close();
            conn.Close();

            output = JsonConvert.SerializeObject(info);
            return output;
        }


        [HttpGet]
        [ActionName("teams")]
        public string GetTeams(string name)
        {
            dynamic info = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;

            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query
            string sql = "SELECT Team.name, Sport.name " +
                "FROM User, UserTeam, Team, Sport " +
                "WHERE User.usrname = '" + name + "' " +
                "AND User.ID = UserTeam.UsrID AND Team.ID = UserTeam.TeamID " +
                "AND Team.sportID = Sport.ID";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                info.Add(new JObject(
                    new JProperty("name", rdr[0]),
                    new JProperty("sport", rdr[1]))
                    );
            }
            rdr.Close();
            conn.Close();

            output = JsonConvert.SerializeObject(info);
            return output;
        }

        // api/user/schedule/Detroit Pistons
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
                "FROM User, UserTeam, Team, Game, Sport " +
                "WHERE User.usrname = '" + name + "' " +
                "AND User.ID = UserTeam.UsrID AND Team.ID = UserTeam.TeamID " +
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

        // POST: api/User/new?name=Michael Jordan&password=abc&email=mj@gmail.com
        [Route("api/user/new")]
        [HttpPost]
        public void NewUser(string name, string password, string email)
        {
            MySql.Data.MySqlClient.MySqlConnection conn;

            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            string sql = "INSERT INTO User (usrname, pw, email) VALUES (" + name + "', '" + password + "', '" + email + "')";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        // PUT: api/User/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/User/5
        public void Delete(int id)
        {
        }

        /*        [HttpGet]
        [ActionName("info")]
        public string Get(string name)
        {
            return "worked";
        }*/
    }
}
