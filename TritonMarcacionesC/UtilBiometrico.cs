using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZKSoftwareAPI;
using Npgsql;
using System.Configuration;

namespace TritonMarcacionesC
{
    public class UtilBiometrico
    {
        ZKSoftware dispositivo;

        Int32 biometrico_id;

        string parametros = ConfigurationManager.ConnectionStrings["tritonPgsql"].ConnectionString;

        NpgsqlConnection conexion1 = new NpgsqlConnection();
        NpgsqlConnection conexion2 = new NpgsqlConnection();
        NpgsqlConnection conexion3 = new NpgsqlConnection();
        NpgsqlConnection conexion4 = new NpgsqlConnection();
        
        public UtilBiometrico()
        {
            dispositivo = new ZKSoftware(Modelo.X628C);
        }

        public void Marcaciones()
        {
            bool conexion_db_1 = ConexionDB(conexion1);

            if (conexion_db_1)
            {
                string sql1 = "SELECT id, ip FROM rrhh_biometricos WHERE estado=4;";

                NpgsqlCommand comando1 = new NpgsqlCommand(sql1, conexion1);

                NpgsqlDataReader consulta1 = comando1.ExecuteReader();
                while (consulta1.Read())
                {
                    biometrico_id = Convert.ToInt32(consulta1[0]);
                    string ip = Convert.ToString(consulta1[1]);

                    //Console.WriteLine(ip);

                    bool conexion_sw = Conectar(ip, 0, false);

                    if (conexion_sw)
                    {
                        if (dispositivo.DispositivoObtenerRegistrosAsistencias())
                        {
                            bool insert_sw = false;
                            string insert_sql = "";
                            //int c = 1;
                            foreach (UsuarioMarcaje marcacion in dispositivo.ListaMarcajes)
                            {
                                DateTime f_marcacion = new DateTime(marcacion.Anio, marcacion.Mes, marcacion.Dia, marcacion.Hora, marcacion.Minuto, marcacion.Segundo);
                                string n_documento = marcacion.NumeroCredencial.ToString();

                                bool conexion_db_2 = ConexionDB(conexion2);

                                if(conexion_db_2)
                                {
                                    string sql2 = "SELECT id FROM rrhh_personas WHERE estado=1 AND n_documento='" + n_documento + "';";
                                    NpgsqlCommand comando2 = new NpgsqlCommand(sql2, conexion2);
                                    NpgsqlDataReader consulta2 = comando2.ExecuteReader();
                                    while (consulta2.Read())
                                    {
                                        bool conexion_db_3 = ConexionDB(conexion3);
                                        if(conexion_db_3)
                                        {
                                            string sql3 = "SELECT COUNT(*) AS cantidad FROM rrhh_log_marcaciones WHERE biometrico_id=" + biometrico_id + " AND persona_id=" + consulta2[0] + " AND f_marcacion='" + f_marcacion.ToString("yyyy-MM-dd HH':'mm':'ss") + "' ;";
                                            NpgsqlCommand comando3 = new NpgsqlCommand(sql3, conexion3);
                                            Int64 cantidad3 = (Int64)comando3.ExecuteScalar();
                                            if (cantidad3 < 1)
                                            {
                                                insert_sql += " INSERT INTO rrhh_log_marcaciones (biometrico_id, persona_id, tipo_marcacion, n_documento_biometrico, f_marcacion) VALUES(" + biometrico_id + ", " + consulta2[0] + ", 1, '" + n_documento + "', '" + f_marcacion.ToString("yyyy-MM-dd HH':'mm':'ss") + "');";
                                            }
                                            CerrarConexionBD(conexion3);
                                        }
                                    }
                                    CerrarConexionBD(conexion2);
                                }
                                insert_sw = true;
                                insert_sql += "INSERT INTO rrhh_log_marcaciones_backup (biometrico_id, tipo_marcacion, n_documento_biometrico, f_marcacion) VALUES(" + biometrico_id + ", 1, '" + n_documento + "', '" + f_marcacion.ToString("yyyy-MM-dd HH':'mm':'ss") + "');";
                                //Console.WriteLine("N° " + c++ + " ID " + n_documento + " FECHA MARCACION " + f_marcacion);
                            }

                            if (insert_sw)
                            {
                                bool conexion_db_4 = ConexionDB(conexion4);
                                if (conexion_db_4)
                                {
                                    insert_sql += " UPDATE rrhh_biometricos SET e_conexion=1, fs_conexion='" + DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss") + "' WHERE id=" + biometrico_id + ";";
                                    NpgsqlCommand comando4 = conexion4.CreateCommand();
                                    comando4.CommandText = insert_sql;
                                    comando4.ExecuteNonQuery();

                                    CerrarConexionBD(conexion4);

                                    if (!dispositivo.DispositivoBorrarRegistrosAsistencias())
                                    {
                                        Console.WriteLine(dispositivo.ERROR);
                                    }
                                }
                            }
                        }
                        Desconectar();
                    }
                }
                CerrarConexionBD(conexion1);
            }
        }
        
        private bool Conectar(String ip, int intentos, bool alerta)
        {
            if (!dispositivo.DispositivoConectar(ip, intentos, alerta))
            {
                Console.WriteLine(dispositivo.ERROR);
                bool conexion_db_4 = ConexionDB(conexion4);
                if (conexion_db_4)
                {
                    string sql = "INSERT INTO rrhh_log_alertas (biometrico_id, tipo_emisor, tipo_alerta, f_alerta, mensaje) VALUES(" + biometrico_id + ", 1, 1, '" + DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss") + "', '" + dispositivo.ERROR + "');";
                    sql += " UPDATE rrhh_biometricos SET e_conexion=2, fs_conexion='" + DateTime.Now.ToString("yyyy-MM-dd HH':'mm':'ss") + "' WHERE id=" + biometrico_id + ";";
                    NpgsqlCommand comando4 = conexion4.CreateCommand();
                    comando4.CommandText = sql;
                    comando4.ExecuteNonQuery();

                    CerrarConexionBD(conexion4);
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void Desconectar()
        {
            dispositivo.DispositivoDesconectar();
        }

        //=== BASE DE DATOS ===
        private bool ConexionDB(NpgsqlConnection conexion)
        {
            conexion.ConnectionString = parametros;

            try
            {
                conexion.Open();
                return true;
            }
            catch(Exception error)
            {
                Console.WriteLine(error);
                return false;
            }
        }

        private void CerrarConexionBD(NpgsqlConnection conexion)
        {
            conexion.Close();
        }
    }
}
