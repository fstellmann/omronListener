using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace omronListener
{
    public static class internalSQL
    {
        public static string connectionString;
        public static ILogger<Worker> _logger;

        public static List<Worker.action> getCommandList()
        {
            List<Worker.action> ret = new List<Worker.action>();
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                try
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = con;
                    command.CommandText = $@"SELECT * FROM robot_instruction_list WHERE done = 0";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            Worker.action hold = new Worker.action();
                            hold.id = reader.GetInt32(0);
                            hold.ip = reader.GetString(1);
                            hold.port = reader.GetInt32(2);
                            hold.command = reader.GetString(3);
                            hold.orderID = reader.GetInt32(5);
                            ret.Add(hold);
                        }
                    }
                }
                catch (Exception exc)
                {
                    reportSQLError(exc);
                }
                finally
                {
                    con.Close();
                }
            }
            return ret;
        }
        public static void updateRow(Worker.action a)
        {
            using (MySqlConnection con = new MySqlConnection(connectionString))
            {
                con.Open();
                try
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = con;
                    command.CommandText = $@"UPDATE robot_instruction_list SET done = 1 WHERE ID = {a.id}";
                    command.ExecuteNonQuery();
                }
                catch (Exception exc)
                {
                    reportSQLError(exc);
                }
                finally
                {
                    con.Close();
                }
            }
        }
        public static void reportSQLError(Exception exc)
        {
            _logger.LogError("{exc}", exc);
        }
    }
}
