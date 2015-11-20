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

namespace IntramuralsAPI.Controllers
{
    public class UserController : ApiController
    {

        // GET: api/user/info/Michael Jordan
        [HttpGet]
        [ActionName("info")]
        public string Get(string name)
        {
            dynamic info = new JArray();
            string output = "";

            MySql.Data.MySqlClient.MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=IntraTest;" +
                "Uid=bbd3fdf9969899;Pwd=7c348d21;";


            conn = new MySqlConnection(myConnectionString);
            conn.Open();

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

        // POST: api/User/new?name=Michael Jordan&password=abc&email=mj@gmail.com
        [Route("api/user/new")]
        [HttpPost]
        public void NewUser(string name, string password, string email)
        {
            MySql.Data.MySqlClient.MySqlConnection conn;
            string myConnectionString;

            // establish connection to database server
            myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=IntraTest;" +
                "Uid=bbd3fdf9969899;Pwd=7c348d21;";

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
