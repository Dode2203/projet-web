using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Fournisseur.Data
{
    public class DatabaseAccess
    {
        private readonly string? _connectionString;

        public DatabaseAccess(IConfiguration configuration)
        {
            // Récupération de la chaîne de connexion depuis le fichier de configuration
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Méthode pour exécuter une requête SELECT
        public DataTable ExecuteQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    var dataTable = new DataTable();
                    var adapter = new NpgsqlDataAdapter(command);

                    connection.Open();
                    adapter.Fill(dataTable);

                    return dataTable;
                }
            }
        }

        // Méthode pour exécuter une commande INSERT, UPDATE ou DELETE
        public int ExecuteNonQuery(string query, params NpgsqlParameter[] parameters)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        // Méthode pour exécuter une commande et retourner un seul résultat (par exemple, une valeur unique)
        public object? ExecuteScalar(string query, params NpgsqlParameter[] parameters)
        {
             using (var connection = new NpgsqlConnection(_connectionString)){
                using (var command = new NpgsqlCommand(query, connection)){
                    if (parameters != null){
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    var result = command.ExecuteScalar();

            // Vérifier si le résultat est DBNull.Value, auquel cas retourner null
                    if (result == DBNull.Value)
                    {
                        return null;
                    }

                    // Si le résultat est un entier valide, retourner sa valeur
                    return result ;
                }
            }
           
        }

         // Méthode pour exécuter une requête SELECT et retourner un IDataReader
        public IDataReader ExecuteReader(string query, params NpgsqlParameter[] parameters)
        {
            var connection = new NpgsqlConnection(_connectionString);
            try
            {
                var command = new NpgsqlCommand(query, connection);
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                // CommandBehavior.CloseConnection garantit que la connexion se ferme lorsque le DataReader est fermé
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

    }
}
