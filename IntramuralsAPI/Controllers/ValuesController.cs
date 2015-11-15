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

namespace IntramuralsAPI.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {

            MySql.Data.MySqlClient.MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "Server=us-cdbr-azure-northcentral-a.cleardb.com;Database=IntraTest;" +
                "Uid=bbd3fdf9969899;Pwd=7c348d21;";


            conn = new MySqlConnection(myConnectionString);
            conn.Open();

            string sql = "SELECT * FROM User";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            string myString = "";

            // Data is accessible through the DataReader object here.
            while (rdr.Read())
            {
                myString += rdr[0].ToString() + rdr[1].ToString();
            }
            rdr.Close();

            return myString;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
