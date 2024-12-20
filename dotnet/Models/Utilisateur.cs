using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Fournisseur.Data;
using System.Net.Mail;
namespace Fournisseur.Models
{
    public class Utilisateur : Inscription{
        public new string Mdp { get; set; } = string.Empty; // Propriété spécifique à Utilisateur

        private readonly DatabaseAccess? _databaseAccess;
        private readonly PinService? _PIN;
        private readonly IConfiguration? _configuration;

        // Constructeur par défaut
        public Utilisateur() { }

        // Constructeur avec configuration pour initialiser DatabaseAccess
        public Utilisateur(IConfiguration configuration)
        {
            _databaseAccess = new DatabaseAccess(configuration) ?? throw new InvalidOperationException("Database access could not be initialized.");
            _PIN = new PinService(configuration);
            _configuration = configuration;
        }

        // Méthode pour afficher les détails
        public override string ToString()
        {
            return base.ToString() + $", Mot de passe : {Mdp}";
        }

        // Méthode pour enregistrer un utilisateur dans la base de données
        public new string Save(string email, string nom, string prenom, string mdp)
        {
            if (_databaseAccess == null){
                throw new InvalidOperationException("Database access is not initialized.");
            }

            string insertQuery = "INSERT INTO utilisateur (email, nom, prenom, mdp) VALUES (@Email, @Nom, @Prenom, @Mdp)";
            var insertParameters = new[]
            {
                new NpgsqlParameter("@Email", email),
                new NpgsqlParameter("@Nom", nom),
                new NpgsqlParameter("@Prenom", prenom),
                new NpgsqlParameter("@Mdp", mdp)
            };

            _databaseAccess.ExecuteNonQuery(insertQuery, insertParameters);
            return "Succès : l'utilisateur a été validé et ajouté à la table utilisateur.";
        }
        public int Login(string email, string mdp){
            if (_databaseAccess == null){
                throw new InvalidOperationException("Database access is not initialized.");
            }
            // Récupérer tous les utilisateurs avec cet email
            string query = "SELECT id_utilisateur, mdp FROM utilisateur WHERE email = @Email";
            var parameter = new NpgsqlParameter("@Email", email);
            var tentavive= _configuration?.GetSection("Tentative");
            int maxT=tentavive?.GetValue<int>("Max")??0;
            using (var reader = _databaseAccess.ExecuteReader(query, parameter))
            {
                bool foundUser = false; // Indique si un utilisateur avec cet email a été trouvé
                bool passwordMatch = false; // Indique si le mot de passe correspond
                int id_utilisateur=-1;
                while (reader.Read())
                {
                    foundUser = true; // Un utilisateur avec cet email existe
                    id_utilisateur = reader.GetInt32(0);
                    string passwd = reader.GetString(1);
                    Console.WriteLine(_PIN?.CompteTentative(id_utilisateur));
                    if(_PIN?.CompteTentative(id_utilisateur)%(maxT-1)==0){
                        
                        string lienDesuppression=$"http://localhost:5032/api/utilisateur/reinstaller?id_utilisateur={id_utilisateur}";
                        EnvoyerEmailUtilisateur(email, lienDesuppression);
                    }

                    if (VerifyPassword(passwd, mdp))
                    {
                        passwordMatch = true; // Le mot de passe correspond

                        // Générer un lien et envoyer un email
                        string lien = $"http://localhost:5032/api/utilisateur/valider?id_utilisateur={id_utilisateur}";
                        EnvoyerEmailUtilisateur(email, lien);
                        // Retourner le lien pour confirmation
                        return id_utilisateur;
                    }
                }
                // Si un utilisateur est trouvé mais le mot de passe est incorrect
                if (foundUser && !passwordMatch){
                    _PIN?.Tentative(id_utilisateur);
                    return 0;
                }
            }

            // Si aucun utilisateur n'est trouvé
            return -1;
        }

        public  string  ValidationUtilisateur(int login){
            if(login==0){
                return "ERREUR : Mots de pass incorect";
            }
            else if(login==-1){
                return "ERREUR : Email  incorect";
            }
            _PIN?.SupprimerTentativesParId(login);
            return "Vous etes connectee";
        }
        public string Reinitialisation(int id){

            _PIN?.SupprimerTentativesParId(id);
            return "Nombre de tentative reinstaliser";
        } 
        public void EnvoyerEmailUtilisateur(string email, string validationLink)
        {
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new System.Net.NetworkCredential("dinantsoa70@gmail.com", "kauuzoxefjyflqvo");
                    smtpClient.EnableSsl = true;

                    // Construire le corps de l'email en HTML
                    string emailBody = $@"
                        <html>
                        <body>
                            <h2>Validation de votre accès</h2>
                            <p>Pour confirmer votre identité, veuillez cliquer sur le bouton ci-dessous :</p>
                            <a href='{validationLink}' 
                            style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #ffffff; background-color: #4CAF50; text-decoration: none; border-radius: 5px;'>
                            Oui, Tatez cette lien {validationLink}
                            </a>
                            <p>Si vous n'avez pas fait cette demande, vous pouvez ignorer cet e-mail.</p>
                        </body>
                        </html>";

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("dinantsoa70@gmail.com"),
                        Subject = "Confirmation de votre identité",
                        Body = emailBody,
                        IsBodyHtml = true, // Spécifie que le contenu est en HTML
                    };

                    mailMessage.To.Add(email);

                    // Envoyer l'email
                    smtpClient.Send(mailMessage);
                    Console.WriteLine("E-mail envoyé avec succès.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de l'email : {ex.Message}");
                throw;
            }
        }

    }

}
