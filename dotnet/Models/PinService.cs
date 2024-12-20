using Npgsql;
using Fournisseur.Data;
using Microsoft.Extensions.Configuration;
namespace Fournisseur.Models;
public class PinService
{
    private readonly DatabaseAccess? _databaseAccess;
    public PinService(){}
    public PinService(IConfiguration configuration){
        _databaseAccess = new DatabaseAccess(configuration) ?? throw new InvalidOperationException("Database access could not be initialized.");;
    }

    // Méthode pour récupérer le code à partir de l'id_inscription
    public int GetCodeByIdInscription(int idInscription){
        if (_databaseAccess == null){
            throw new InvalidOperationException("DatabaseAccess n'est pas initialisé.");
        }

        // Requête SQL pour récupérer le code correspondant à l'id_inscription
        string query = "SELECT code FROM pin WHERE id_inscription = @IdInscription LIMIT 1";

        // Paramètres pour la requête SQL
        var parameters = new[] { new NpgsqlParameter("@IdInscription", idInscription) };

        // Exécuter la requête avec ExecuteScalar et récupérer le code
        var result = _databaseAccess.ExecuteScalar(query, parameters);

        // Vérifier si le résultat est non null et de type int
        if (result != null && result is int)
        {
            return (int)result; // Retourner le code trouvé
        }

        return -1; // Retourner -1 si aucun code n'a été trouvé
    }
    public int  Verifier(int id,int code){
         if (_databaseAccess == null){
            throw new InvalidOperationException("DatabaseAccess n'est pas initialisé.");
        }
        string pinQuery = "SELECT COUNT(*) FROM pin WHERE id_inscription = @Id AND code = @Code";
                var pinParameters = new[]
                {
                    new NpgsqlParameter("@Id", id),
                    new NpgsqlParameter("@Code", code)
                };
                object? pinResult = _databaseAccess.ExecuteScalar(pinQuery, pinParameters);
                if (pinResult == null || pinResult == DBNull.Value){
                    Console.WriteLine("pinResult is null or DBNull.");
                    return 0;
                }
                int pinCount = Convert.ToInt32(pinResult);
                Console.WriteLine($"pinCount: {pinCount}");
                return pinCount;
                
    } 
    public string SupprimerPinParId(int idInscription){
             if (_databaseAccess == null)
            {
                // Si _databaseAccess est nul, retourner une erreur
               throw new InvalidOperationException("Database access is not initialized.");
            }
            try
            {
                // Requête SQL pour supprimer l'entrée dans la table 'pin' correspondant à l'id_inscription
                string deleteQuery = "DELETE FROM pin WHERE id_inscription = @IdInscription";

                // Paramètres de la requête
                var parameters = new[]
                {
                    new NpgsqlParameter("@IdInscription", NpgsqlTypes.NpgsqlDbType.Integer) { Value = idInscription }
                };

                // Appel de la méthode ExecuteNonQuery pour exécuter la requête
                int rowsAffected = _databaseAccess.ExecuteNonQuery(deleteQuery, parameters);

                // Vérifier si une ligne a été affectée (supprimée)
                if (rowsAffected > 0)
                {
                    return "Le code PIN a été supprimé avec succès.";
                }
                else
                {
                    return "Aucune entrée trouvée pour l'ID d'inscription spécifié.";
                }
            }
            catch (Exception ex)
            {
                // Gestion des erreurs
                return $"Erreur lors de la suppression du PIN : {ex.Message}";
            }
        }
        public int SavePin(int id, int code){
            if (_databaseAccess == null){
                throw new InvalidOperationException("Database access is not initialized.");
            }

            string insertQuery = "INSERT INTO pin (id_inscription, code) VALUES (@id, @code) RETURNING id";

            var parameters = new[]
            {
                new NpgsqlParameter("@id", id),
                new NpgsqlParameter("@code", code)
            };

            // Appeler DatabaseAccess pour exécuter la requête INSERT et obtenir l'ID généré
            object? result = _databaseAccess.ExecuteScalar(insertQuery, parameters);

            return result != null ? Convert.ToInt32(result) : -1;
        }
        public void Tentative(int id){
             if (_databaseAccess == null){
                throw new InvalidOperationException("Database access is not initialized.");
            }
            string insertQuery = "INSERT INTO tentative (id_utilisateur, nombre) VALUES (@id, @nombre)";
             var parameters = new[]
            {
                new NpgsqlParameter("@id", id),
                new NpgsqlParameter("@nombre", 1)
            };
            _databaseAccess.ExecuteNonQuery(insertQuery, parameters);
        }
        public int CompteTentative(int id){
            if (_databaseAccess == null){
                throw new InvalidOperationException("Database access is not initialized.");
            }

            string query = "SELECT COUNT(*) FROM tentative WHERE id_utilisateur = @id";
             var parameters = new[]
            {
                new NpgsqlParameter("@id", id)
            };
            using (var reader = _databaseAccess.ExecuteReader(query,parameters))
            {
                if (reader.Read()){
                    return reader.GetInt32(0); // Retourne le nombre de lignes dans la table
                }
            }
            throw new Exception("Erreur lors du comptage des lignes dans la table tentative.");
        }
        public void SupprimerTentativesParId(int id)
        {
            if (_databaseAccess == null)
            {
                throw new InvalidOperationException("Database access is not initialized.");
            }

            // Requête SQL pour supprimer les lignes correspondantes
            string deleteQuery = "DELETE FROM tentative WHERE id_utilisateur = @id";
            var parameters = new[]
            {
                new NpgsqlParameter("@id", id)
            };

            // Exécute la requête
            int rowsAffected = _databaseAccess.ExecuteNonQuery(deleteQuery, parameters);

            // Affiche le résultat
            Console.WriteLine($"{rowsAffected} ligne(s) supprimée(s) pour l'id_utilisateur {id}.");
        }


}
