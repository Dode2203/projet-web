using System.Data;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Fournisseur.Data;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Fournisseur.Models
{
    public class Inscription
    {
        [Column("id_inscription")] 
        public int? Id { get; set; }
        public string Email { get; set; } = String.Empty;
        public string Nom { get; set; } = String.Empty;
        public string Prenom { get; set; } = String.Empty;
        public string Mdp { get; set; } = String.Empty;

        
        // Constructeur par défaut
        public Inscription() { }

        // Constructeur avec paramètres
        public Inscription(int id, string email, string nom, string prenom, string mdp)
        {
            Id = id;
            Email = email;
            Nom = nom;
            Prenom = prenom;
            Mdp = mdp;
        }

        private readonly DatabaseAccess? _databaseAccess;
        private readonly PinService? _PIN;
        private readonly Utilisateur? _Utilisateur;
        public Inscription(IConfiguration configuration)
        {
            // Initialiser DatabaseAccess avec la configuration de la chaîne de connexion
            _databaseAccess = new DatabaseAccess(configuration) ?? throw new InvalidOperationException("Database access could not be initialized.");
            _PIN = new PinService(configuration);
            _Utilisateur= new Utilisateur(configuration);
        }

        // Méthode pour afficher les détails
        public override string ToString()
        {
            return $"Id: {Id}, Email: {Email}, Nom: {Nom}, Prénom: {Prenom}";
        }

        public void EnvoyerEmail(string email, int codePIN)
        {
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new System.Net.NetworkCredential("dinantsoa70@gmail.com", "kauuzoxefjyflqvo");
                    smtpClient.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("dinantsoa70@gmail.com"),
                        Subject = "Code de validation",
                        Body = $"Votre code de validation est : {codePIN}",
                        IsBodyHtml = false,
                    };
                    mailMessage.To.Add(email);

                    smtpClient.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email : {ex.Message}");
                throw;
            }
        }

        public int Save(string email, string nom, string prenom, string mdp)
        {
            if (_databaseAccess == null)
            {
                throw new InvalidOperationException("Database access is not initialized.");
            }

            string insertQuery = "INSERT INTO inscription (email, nom, prenom, mdp) VALUES (@Email, @Nom, @Prenom, @Mdp) RETURNING id_inscription";
            string hashedPassword = HashPassword(mdp);
            var parameters = new[]
            {
                new NpgsqlParameter("@Email", email),
                new NpgsqlParameter("@Nom", nom),
                new NpgsqlParameter("@Prenom", prenom),
                new NpgsqlParameter("@Mdp", hashedPassword)
            };

            // Appeler DatabaseAccess pour exécuter la requête INSERT et obtenir l'ID généré
            object? result = _databaseAccess.ExecuteScalar(insertQuery, parameters);

            return result != null ? Convert.ToInt32(result) : -1;
        }

        
        public string SInscrire(string email, string nom, string prenom, string mdp)
        {
            // Sauvegarde de l'utilisateur dans la base de données
            if(EmailExiste(email)){
                 throw new Exception("Email existant.");
            }
            int idInscription = Save(email, nom, prenom, mdp);
            
            if (idInscription <= 0)
            {
                throw new Exception("Échec de la sauvegarde de l'inscription.");
            }

            // Génération d'un code PIN
            Random random = new Random();
            int codePIN = random.Next(100000, 999999);
            int savepin = _PIN?.SavePin(idInscription, codePIN)??0;
            Console.WriteLine($"Id: {savepin}, Code: {codePIN}");
            // Envoi du code PIN à l'adresse email
            EnvoyerEmail(email, codePIN);

            // Génération du lien de validation
            string lienValidation = $"localhost:5032/api/Inscription/validation?id_inscription={idInscription}&code= (remplacer par la code dans l'email)";
            return lienValidation;
        }

        public string Validation(int id, int code){
            if (_databaseAccess == null){
                throw new InvalidOperationException("Database access is not initialized.");
            }

            try{
                 // Initialiser IConfiguration pour `PinService`
            
                int pinCount = _PIN?.Verifier(id,code) ?? 0;
                if (pinCount == 0){
                    return "Erreur : le code PIN est invalide.";
                }
                // Récupérer les informations de l'utilisateur à partir de la table inscription
                string selectQuery = "SELECT email, nom, prenom, mdp FROM inscription WHERE id_inscription = @Id";
                var selectParameters = new[]
                {
                    new NpgsqlParameter("@Id", id)
                };

                using (var reader = _databaseAccess.ExecuteReader(selectQuery, selectParameters))
                {
                    if (reader.Read())
                    {
                        string email = reader.GetString(0);
                        string nom = reader.GetString(1);
                        string prenom = reader.GetString(2);
                        string mdp = reader.GetString(3);

                        // Insérer les informations dans la table utilisateur
                        string insererUtilisateur=_Utilisateur?.Save(email,nom,prenom,mdp)??"";
                        string delete=_PIN?.SupprimerPinParId(id)??"";
                        Console.WriteLine(insererUtilisateur);
                        Console.WriteLine(delete);
                        return insererUtilisateur;

                    }   
                    else
                    {
                        return "Erreur : aucune inscription trouvée pour cet ID.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Gestion des erreurs
                return $"Erreur : {ex.Message}";
            }
        }
       public  string HashPassword(string password){
            // Utiliser la classe PasswordHasher de .NET pour hacher le mot de passe
            var hasher = new PasswordHasher<object>();
            var dummyUser = new object(); // Crée un objet factice
            return hasher.HashPassword(dummyUser, password);  // Utilisation d'un objet factice
        }
        public bool VerifyPassword(string hashedPassword, string passwordToVerify)
        {
            var hasher = new PasswordHasher<object>();
            var dummyUser = new object(); // Crée un objet factice

            // Vérifier si le mot de passe correspond au hachage
            var result = hasher.VerifyHashedPassword(dummyUser, hashedPassword, passwordToVerify);
            return result == PasswordVerificationResult.Success;
        }

        public bool EmailExiste(string email)
        {
            if (_databaseAccess == null)
            {
                throw new InvalidOperationException("Database access is not initialized.");
            }

            // Requête SQL pour vérifier si l'email existe
            string query = "SELECT COUNT(*) FROM inscription WHERE email = @Email";
            var parameter = new NpgsqlParameter("@Email", email);

            // Exécuter la requête
            using (var reader = _databaseAccess.ExecuteReader(query, parameter))
            {
                if (reader.Read())
                {
                    int count = reader.GetInt32(0);
                    return count > 0; // Retourne true si l'email existe
                }
            }

            return false; // Retourne false si l'email n'existe pas
        }

    }
    
}
