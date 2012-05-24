using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

using Reactivity.Objects;

namespace Reactivity.Data
{
    public static class StoredProcedure
    {
        #region Device
        public static Device DeviceGetByGuid(Guid guid, Connection connection)
        {
            SqlCommand cmd = _setup("r_device_get_by_guid", connection);
            _param(cmd, "@guid", SqlDbType.VarChar, guid.ToString());
            Device device = null;
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                device = new Device
                {
                    Guid = new Guid(reader["r_guid"].ToString()),
                    Name = reader["r_name"].ToString(),
                    Description = reader["r_description"].ToString(),
                    Type = new Guid(reader["r_type"].ToString()),
                    Profile = reader["r_profile"].ToString(),
                    Configuration = reader["r_configuration"].ToString(),
                    Status = DeviceStatus.Unknown
                };
            reader.Close();
            reader.Dispose();
            return device;
        }

        public static Device[] DeviceListAll(Connection connection)
        {
            SqlCommand cmd = _setup("r_device_list_all", connection);
            List<Device> devices = new List<Device>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                devices.Add(new Device
                {
                    Guid = new Guid(reader["r_guid"].ToString()),
                    Name = reader["r_name"].ToString(),
                    Description = reader["r_description"].ToString(),
                    Type = new Guid(reader["r_type"].ToString()),
                    Profile = reader["r_profile"].ToString(),
                    Configuration = reader["r_configuration"].ToString(),
                    Status = DeviceStatus.Unknown
                });
            reader.Close();
            reader.Dispose();
            return devices.ToArray();
        }

        public static bool DeviceCreate(Guid guid, string name, string description, Guid type, string profile, string configuration, Connection connection)
        {
            SqlCommand cmd = _setup("r_device_create", connection);
            _param(cmd, "@guid", SqlDbType.VarChar, guid.ToString());
            _param(cmd, "@name", SqlDbType.NVarChar, name);
            _param(cmd, "@description", SqlDbType.NVarChar, description);
            _param(cmd, "@type", SqlDbType.VarChar, type.ToString());
            _param(cmd, "@profile", SqlDbType.Xml, profile);
            _param(cmd, "@configuration", SqlDbType.Xml, configuration);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeviceRemoveByGuid(Guid guid, Connection connection)
        {
            SqlCommand cmd = _setup("r_device_remove_by_guid", connection);
            _param(cmd, "@guid", SqlDbType.VarChar, guid.ToString());
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeviceUpdateByGuid(Guid guid, string name, string description, Guid type, string profile, string configuration, Connection connection)
        {
            SqlCommand cmd = _setup("r_device_update_by_guid", connection);
            _param(cmd, "@guid", SqlDbType.VarChar, guid.ToString());
            _param(cmd, "@name", SqlDbType.NVarChar, name);
            _param(cmd, "@description", SqlDbType.NVarChar, description);
            _param(cmd, "@type", SqlDbType.VarChar, type.ToString());
            _param(cmd, "@profile", SqlDbType.Xml, profile);
            _param(cmd, "@configuration", SqlDbType.Xml, configuration);
            return cmd.ExecuteNonQuery() > 0;
        }
        #endregion

        #region Rule
        public static Objects.Rule RuleGetById(int id, Connection connection)
        {
            SqlCommand cmd = _setup("r_rule_get_by_id", connection);
            _param(cmd, "@id", SqlDbType.Int, id);
            Objects.Rule rule = null;
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                rule = new Objects.Rule { ID = Convert.ToInt32(reader["id"]), Name = reader["r_name"].ToString(), Description = reader["r_description"].ToString(), Configuration = reader["r_configuration"].ToString(), Precedence = Convert.ToInt32(reader["r_precedence"]), IsEnabled = Convert.ToBoolean(reader["r_enable"]) };
            reader.Close();
            reader.Dispose();
            return rule;
        }

        public static Objects.Rule[] RuleListAll(Connection connection)
        {
            SqlCommand cmd = _setup("r_rule_list_all", connection);
            List<Objects.Rule> rules = new List<Objects.Rule>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                rules.Add(new Objects.Rule { ID = Convert.ToInt32(reader["id"]), Name = reader["r_name"].ToString(), Description = reader["r_description"].ToString(), Configuration = reader["r_configuration"].ToString(), Precedence = Convert.ToInt32(reader["r_precedence"]), IsEnabled = Convert.ToBoolean(reader["r_enable"]) });
            reader.Close();
            reader.Dispose();
            return rules.ToArray();
        }

        public static int RuleCreate(string name, string description, string configuration, int precedence, bool enable, Connection connection)
        {
            SqlCommand cmd = _setup("r_rule_create", connection);
            _param(cmd, "@name", SqlDbType.NVarChar, name);
            _param(cmd, "@description", SqlDbType.NVarChar, description);
            _param(cmd, "@configuration", SqlDbType.Xml, configuration);
            _param(cmd, "@precedence", SqlDbType.Int, precedence);
            _param(cmd, "@enable", SqlDbType.Bit, enable);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static bool RuleRemoveById(int id, Connection connection)
        {
            SqlCommand cmd = _setup("r_rule_remove_by_id", connection);
            _param(cmd, "@id", SqlDbType.Int, id);
            return cmd.ExecuteNonQuery()>0;
        }

        public static bool RuleUpdateById(int id, string name, string description, string configuration, int precedence, bool enable, Connection connection)
        {
            SqlCommand cmd = _setup("r_rule_update_by_id", connection);
            _param(cmd, "@id", SqlDbType.Int, id);
            _param(cmd, "@name", SqlDbType.NVarChar, name);
            _param(cmd, "@description", SqlDbType.NVarChar, description);
            _param(cmd, "@configuration", SqlDbType.Xml, configuration);
            _param(cmd, "@precedence", SqlDbType.Int, precedence);
            _param(cmd, "@enable", SqlDbType.Bit, enable);
            return cmd.ExecuteNonQuery() > 0;
        }
        #endregion

        #region User
        /// <summary>
        /// Retrieve the password for this user in order to authenticate
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="connection">Database connection</param>
        /// <returns>User password, null if user not exists</returns>
        public static string UserPasswordGetByUsername(string username, Connection connection)
        {
            SqlCommand cmd = _setup("r_user_password_get_by_username", connection);
            _param(cmd, "@username", SqlDbType.VarChar, username);
            object result = cmd.ExecuteScalar();
            return result == null ? null : result.ToString();
        }

        /// <summary>
        /// Retrieve the password for this user in order to authenticate
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="connection">Database connection</param>
        /// <returns>User password, null if user not exists</returns>
        public static string UserPasswordGetById(int id, Connection connection)
        {
            SqlCommand cmd = _setup("r_user_password_get_by_id", connection);
            _param(cmd, "@id", SqlDbType.Int, id);
            object result = cmd.ExecuteScalar();
            return result == null ? null : result.ToString();
        }

        /// <summary>
        /// Called when user is authorized to login, retrieve user info
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="connection">Database connection</param>
        /// <returns>Detailed user information</returns>
        public static User UserGetByUsername(string username, Connection connection)
        {
            SqlCommand cmd = _setup("r_user_get_by_username", connection);
            _param(cmd, "@username", SqlDbType.VarChar, username);
            User user = null;
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                user = new User
                {
                    ID = Convert.ToInt32(reader["id"]),
                    Username = reader["r_username"].ToString(),
                    Name = reader["r_name"].ToString(),
                    Description = reader["r_description"].ToString(),
                    Permission = Convert.ToInt32(reader["r_permission"])
                };
            reader.Close();
            reader.Dispose();
            return user;
        }

        public static User UserGetById(int id, Connection connection)
        {
            SqlCommand cmd = _setup("r_user_get_by_id", connection);
            _param(cmd, "@id", SqlDbType.Int, id);
            User user = null;
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                user = new User
                { 
                    ID = Convert.ToInt32(reader["id"]),
                    Username = reader["r_username"].ToString(),
                    Name = reader["r_name"].ToString(),
                    Description = reader["r_description"].ToString(),
                    Permission = Convert.ToInt32(reader["r_permission"])
                };
            reader.Close();
            reader.Dispose();
            return user;
        }

        public static User[] UserListAll(Connection connection)
        {
            SqlCommand cmd = _setup("r_user_list_all", connection);
            List<User> users = new List<User>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
                users.Add(new User
                {
                    ID = Convert.ToInt32(reader["id"]),
                    Username = reader["r_username"].ToString(),
                    Name = reader["r_name"].ToString(),
                    Description = reader["r_description"].ToString(),
                    Permission = Convert.ToInt32(reader["r_permission"])
                });
            reader.Close();
            reader.Dispose();
            return users.ToArray();
        }

        public static bool UserRemoveById(int id, Connection connection)
        {
            SqlCommand cmd = _setup("r_user_remove_by_id", connection);
            _param(cmd, "@id", SqlDbType.Int, id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static int UserCreate(string username, string name, string description, int permission, Connection connection)
        {
            SqlCommand cmd = _setup("r_user_create", connection);
            _param(cmd, "@username", SqlDbType.VarChar, username);
            _param(cmd, "@name", SqlDbType.NVarChar, name);
            _param(cmd, "@description", SqlDbType.NVarChar, description);
            _param(cmd, "@permission", SqlDbType.Int, permission);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static bool UserUpdateById(int id, string name, string description, int permission, Connection connection)
        {
            SqlCommand cmd = _setup("r_user_update_by_id", connection);
            _param(cmd, "@id", SqlDbType.Int, id);
            _param(cmd, "@name", SqlDbType.NVarChar, name);
            _param(cmd, "@description", SqlDbType.NVarChar, description);
            _param(cmd, "@permission", SqlDbType.Int, permission);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool UserPasswordUpdateById(int id, string password, Connection connection)
        {
            SqlCommand cmd = _setup("r_user_password_update_by_id", connection);
            _param(cmd, "@id", SqlDbType.Int, id);
            _param(cmd, "@password", SqlDbType.VarChar, password);
            return cmd.ExecuteNonQuery() > 0;
        }
        #endregion

        #region Statistics
        public static void StatisticsAdd(Guid device, short service, DateTime date, long count, double value, Connection connection)
        {
            SqlCommand cmd = _setup("r_statistics_add", connection);
            _param(cmd, "@device", SqlDbType.VarChar, device.ToString());
            _param(cmd, "@service", SqlDbType.SmallInt, service);
            _param(cmd, "@date_minute", SqlDbType.DateTime, new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0));
            _param(cmd, "@date_hour", SqlDbType.DateTime, new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0));
            _param(cmd, "@date_day", SqlDbType.DateTime, new DateTime(date.Year, date.Month, date.Day));
            _param(cmd, "@date_month", SqlDbType.DateTime, new DateTime(date.Year, date.Month, 1));
            _param(cmd, "@count", SqlDbType.BigInt, count);
            _param(cmd, "@value", SqlDbType.Real, value);
            cmd.ExecuteNonQuery();
        }
        public static void StatisticsClean(int minute, int hour, int day, int month, Connection connection)
        {
            SqlCommand cmd = _setup("r_statistics_clean", connection);
            _param(cmd, "@minute", SqlDbType.Int, minute);
            _param(cmd, "@hour", SqlDbType.Int, hour);
            _param(cmd, "@day", SqlDbType.Int, day);
            _param(cmd, "@month", SqlDbType.Int, month);
            cmd.ExecuteNonQuery();
        }
        public static Objects.Statistics[] StatisticsQuery(Guid device, short service, DateTime start_date, DateTime end_date, Objects.StatisticsType type, Connection connection)
        {
            SqlCommand cmd = _setup("r_statistics_query", connection);
            _param(cmd, "@device", SqlDbType.VarChar, device.ToString());
            _param(cmd, "@service", SqlDbType.SmallInt, service);
            _param(cmd, "@start_date", SqlDbType.DateTime, start_date);
            _param(cmd, "@end_date", SqlDbType.DateTime, end_date);
            _param(cmd, "@type", SqlDbType.TinyInt, Convert.ToByte(type));
            List<Objects.Statistics> stats = new List<Objects.Statistics>();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                Objects.Statistics stat = new Objects.Statistics
                {
                    Device = new Guid(reader["r_device"].ToString()),
                    Service = Convert.ToInt16(reader["r_service"]),
                    Date = Convert.ToDateTime(reader["r_date"]),
                    Count = Convert.ToInt64(reader["r_count"]),
                    Value = Convert.ToDouble(reader["r_value"])
                };
                byte r_type = Convert.ToByte(reader["r_type"]);
                if (r_type == 1) stat.Type = StatisticsType.Minutely;
                else if (r_type == 2) stat.Type = StatisticsType.Hourly;
                else if (r_type == 3) stat.Type = StatisticsType.Daily;
                else if (r_type == 4) stat.Type = StatisticsType.Monthly;
                stats.Add(stat);
            }
            reader.Close();
            reader.Dispose();
            return stats.ToArray();
        }
        #endregion

        #region Helpers
        private static SqlCommand _setup(string name, Connection connection)
        {
            if (connection == null || connection.connection == null)
                throw new ArgumentException("Database connection not open!");
            if (connection.connection.State != ConnectionState.Open)
                throw new ArgumentException("Database connection not open!");
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = name;
            cmd.Connection = connection.connection;
            return cmd;
        }
        private static void _param(SqlCommand cmd, string name, SqlDbType type, object value)
        {
            if (!cmd.Parameters.Contains(name))
                cmd.Parameters.Add(name, type);
            cmd.Parameters[name].SqlDbType = type;
            cmd.Parameters[name].Value = value;
        }
        #endregion
    }
}
