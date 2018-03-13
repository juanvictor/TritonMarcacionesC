using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Npgsql;
using System.Data;

namespace TritonMarcacionesC
{
    public abstract class ConexionPgsql
    {
        private static NpgsqlConnection enlace { get; set; }

        public static NpgsqlConnection getConexion()
        {
            if(enlace == null)
            {
                enlace = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["LINQ_PGSQL"].ToString());
                enlace.Open();
            }

            return enlace;
        }

        private static NpgsqlCommand prepareExecute(string consulta, CommandType tipo, params Parameter[] args)
        {
            NpgsqlCommand cmd = new NpgsqlCommand();

            cmd.Connection = getConexion();
            cmd.CommandType = tipo;
            cmd.CommandText = consulta;

            foreach(Parameter argument in args)
            {
                cmd.Parameters.Add(new NpgsqlParameter(argument.key, argument.value));
            }

            if(cmd.Connection.State == ConnectionState.Closed)
            {
                cmd.Connection.Open();
            }

            return cmd;
        }

        public static IDataReader executeOperation(string consulta, CommandType tipo = CommandType.Text)
        {
            using (NpgsqlCommand cmd = prepareExecute(consulta, tipo))
            {
                return cmd.ExecuteReader();
            }
        }

        public static IDataReader executeOperation(string consulta, CommandType tipo, params Parameter[] args)
        {
            using (NpgsqlCommand cmd = prepareExecute(consulta, tipo, args))
            {
                return cmd.ExecuteReader();
            }
        }
    }
}
