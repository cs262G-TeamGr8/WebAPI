using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IntramuralsAPI.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        /*public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }*/

        // GET api/values/players/Detroit Pistons
        [HttpGet]
        [ActionName("players")]
        public string Get(string name)
        {
            dynamic players = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=IntraTest;" +
                "Uid=bbd3fdf9969899;Pwd=7c348d21;";


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

        // POST api/values
        /*[HttpPost]
        [ActionName("addPlayer")]
        public void Post(string playerName)
        {
            Console.Write("worked");
        }*/

        // PUT api/values/5
        /*public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }*/
    }
}
