using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Fournisseur.Models;
using MyApp.Services;

namespace Fournisseur.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilisateurController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtTokenManager _tokenCreator;

        public UtilisateurController(IConfiguration configuration)
        {
            _configuration = configuration;
            _tokenCreator = new JwtTokenManager();
        }

        [HttpPost("login")]
        [Route("login")]
        // http://localhost:5032/api/utilisateur/login
        public IActionResult Login([FromBody] Utilisateur request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Mdp))
                {
                    var missingFields = new List<string>();
                    if (string.IsNullOrEmpty(request?.Email))
                    {
                        missingFields.Add("Email");
                    }
                    if (string.IsNullOrEmpty(request?.Mdp))
                    {
                        missingFields.Add("Mdp");
                    }

                    return BadRequest(new
                    {
                        status = "error",
                        message = "Missing required fields",
                        missingFields
                    });
                }

                // Instancier l'objet utilisateur
                var utilisateur = new Utilisateur(_configuration);

                // Appeler la méthode Login
                int result = utilisateur.Login(request.Email, request.Mdp);

                return Ok(new
                {
                    status = "success",
                    message = "Login successful",
                    result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Erreur lors de la connexion : {ex.Message}"
                });
            }
        }

        [HttpGet("reinstaller")]
        [Route("reinstaller")]
        // http://localhost:5032/api/utilisateur/reinstaller?id_utilisateur={id_utilisateur}
        public IActionResult Reinstaliser([FromQuery] int id_utilisateur)
        {
            try
            {
                if (id_utilisateur <= 0)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "Invalid or missing parameter: id_utilisateur"
                    });
                }

                var utilisateur = new Utilisateur(_configuration);

                // Appel de la méthode Reinitialisation
                string result = utilisateur.Reinitialisation(id_utilisateur);

                return Ok(new
                {
                    status = "success",
                    message = "Réinitialisation effectuée avec succès",
                    result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Erreur de réinitialisation : {ex.Message}"
                });
            }
        }

        [HttpGet("valider")]
        [Route("valider")]
        // http://localhost:5032/api/utilisateur/valider?id_utilisateur={id_utilisateur}
        public IActionResult Valider([FromQuery] int id_utilisateur)
        {
            try
            {
                if (id_utilisateur <= 0)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "Invalid or missing parameter: id_utilisateur"
                    });
                }

                var utilisateur = new Utilisateur(_configuration);

                // Appel de la méthode ValidationUtilisateur
                string result = utilisateur.ValidationUtilisateur(id_utilisateur);
                 var roles = new List<string> { "ROLE_CLIENT", "ROLE_ADMIN" }; // Liste des rôles

                var claims = new Dictionary<string, object>
                {
                    { "id", id_utilisateur },
                    { "roles", roles } // Ajouter les rôles dans le claims
                };

                
                int expirationInSeconds = 3600; // 1 heure
                string token = _tokenCreator.CreateToken(claims, expirationInSeconds);
                return Ok(new
                {
                    status = "success",
                    message = "Validation effectuée avec succès",
                    result,
                    token=token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Erreur de validation : {ex.Message}"
                });
            }
        }
    }
}
