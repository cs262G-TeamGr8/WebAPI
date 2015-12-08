/*******************************************************************
 *  UserController.cs
 *
 *  Creates all API calls concerning a user.
 *
 ******************************************************************/

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
    /* 
     *  Account class defines an account used to create a new user.
     *  Contains Username, Password, Email.
     */
    public class Account
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    // Allow cross-origin requests from our client, and test clients
    [EnableCors(origins: "http://intramuraltest.azurewebsites.net,http://manji.azurewebsites.net,http://localhost:8080", headers: "*", methods: "*")]
    public class UserController : ApiController
    {
        // establish connection to MySQL server
        string myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=Intramurals;" +
            "Uid=bca5c68bb20976;Pwd=fa8ba175;";

        /* 
         *  GET: api/user/info/Michael Jordan
         *  Retrieves the relevant infomation regarding the specfied user.
         *  @param: string name, specified username to lookup
         *  @return: ID, name, email of username in JSON format
         */
        [HttpGet]
        [ActionName("info")]
        public string Get(string name)
        {
            // initiate JArray for JSON data
            dynamic info = new JArray();
            string output = "";

            // create MySQL connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query command
            string sql = "SELECT * FROM User WHERE User.usrname = '" + name + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                // add JSON data to JArray
                info.Add(new JObject(
                    new JProperty("ID", rdr[0]),
                    new JProperty("name", rdr[1]),
                    new JProperty("email", rdr[3]))
                    );
            }

            rdr.Close();
            conn.Close();

            // serialize JSON data
            output = JsonConvert.SerializeObject(info);
            return output;
        }

        /*
         *  GET: api/user/login?email=example@gmail.com&password=password
         *  Checks if the login information exists in the database.
         *  @param: string email, the email for the account,
         *          string password, the password for the account.
         *  @return: JSON data specifying if account exists or if the password is incorrect.
         */
        [Route("api/user/login")]
        [HttpGet]
        public string CheckLogin(string email, string password)
        {
            // initiate JArray for JSON data
            dynamic info = new JArray();
            string output = "";

            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query command
            string sql = "SELECT User.pw, User.email, User.usrname, User.ID FROM User WHERE User.email = '" + email + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            // Data is accessible through the DataReader object here.
            rdr.Read();

            // account is found, exists
            if (rdr.HasRows)
            {
                // password is correct
                if (rdr[0].ToString() == password)
                {
                    // add all JSON data to JArray
                    info.Add(new JObject(
                        new JProperty("loggedIn", true),
                        new JProperty("email", rdr[1]),
                        new JProperty("name", rdr[2]),
                        new JProperty("id", rdr[3]),
                        new JProperty("message", "Login successful"))
                        );
                }
                // password is incorrect
                else
                {
                    // add only email and message to JSON data
                    info.Add(new JObject(
                        new JProperty("loggedIn", false),
                        new JProperty("email", rdr[1]),
                        new JProperty("name", null),
                        new JProperty("id", null),
                        new JProperty("message", "Password is incorrect"))
                        );
                }
            }
            // account not found, does not exist
            else
            {
                // add only message to JSON data
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

            // serialize JSON data
            output = JsonConvert.SerializeObject(info);
            return output;
        }

        /*
         *  GET: api/user/teams/Michael Jordan
         *  Retrieves teams a user is on
         *  @param: string name, the username of the user.
         *  @return: a list of teams and leagues the user is on in JSON format.
         */
        [HttpGet]
        [ActionName("teams")]
        public string GetTeams(string name)
        {
            // initiate JArray for JSON data
            dynamic info = new JArray();
            string output = "";

            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // sql query command
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
                // add JSON data to JArray
                info.Add(new JObject(
                    new JProperty("name", rdr[0]),
                    new JProperty("sport", rdr[1]))
                    );
            }
            rdr.Close();
            conn.Close();

            // serialize JSON data
            output = JsonConvert.SerializeObject(info);
            return output;
        }

        /*
         *  GET: api/user/schedule/Michael Jordan
         *  Retrieves the individual schedule for a specified user
         *  @param: string name, the username of the user.
         *  @return: a list of individual games (schedule) for the user.
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
                // add each game to the JArray
                // home and away teams are given as ID #'s
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
         *  POST: api/user/new
         *  Creates a new user in the database.
         *  @param: Account acct, an instance of Account class with new user information.
         *  @return: string acct.Username, the name of the new account just created.
         */
        [Route("api/user/new")]
        [HttpPost]
        public string NewUser(Account acct)
        {
            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // create sql command
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO User (usrname, pw, email) VALUES (@username, @password, @email)";

            // add values to parameters in sql command
            cmd.Parameters.AddWithValue("@username", acct.Username);
            cmd.Parameters.AddWithValue("@password", acct.Password);
            cmd.Parameters.AddWithValue("@email", acct.Email);
            cmd.ExecuteNonQuery();

            return acct.Username;
        }

        /*
         *  POST: api/user/join?userId=5&teamName=Bulls
         *  Creates an entry in UserTeam table in database, which adds a user to a team.
         *  @param: int userID, the ID of the user joining a team,
         *          string teamName, the name of the team which the user is joining.
         *  @return: string teamName, the name of the team just joined.
         */
        [Route("api/user/join")]
        [HttpPost]
        public string JoinTeam(int userID, string teamName)
        {
            // create MySql connection
            MySql.Data.MySqlClient.MySqlConnection conn;
            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            // this sql command finds the team ID # associated with the teamName
            string sql = "SELECT ID FROM Team WHERE Team.name = '" + teamName + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            // teamID is now the ID # for the team
            int teamID = Int16.Parse(rdr[0].ToString());
            rdr.Close();

            // create a new sql command for joining a team
            cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO UserTeam (UsrID, TeamID) VALUES (@userID, @teamID)";

            // add values to parameters in sql command
            cmd.Parameters.AddWithValue("@userID", userID);
            cmd.Parameters.AddWithValue("@teamID", teamID);
            cmd.ExecuteNonQuery();

            return teamName;
        }

        // PUT: api/User/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/User/5
        public void Delete(int id)
        {
        }
    }
}
