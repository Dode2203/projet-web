using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Fournisseur.Models;
using System;

namespace Fournisseur.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InscriptionController : ControllerBase
    {
        private readonly Inscription _inscriptionService;

        // Constructeur qui initialise le service d'inscription
        public InscriptionController(IConfiguration configuration)
        {
            _inscriptionService = new Inscription(configuration);
        }

        // Endpoint pour s'inscrire
        [HttpPost]
        [Route("sinscrire")]
        //http://localhost:5032/api/inscription/sinscrire
        public IActionResult SInscrire([FromBody] Inscription model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { status = "error", message = "No data provided" });
                }

                // Liste des champs requis
                var requiredFields = new Dictionary<string, string>
                {
                    { "Email", model.Email },
                    { "Nom", model.Nom },
                    { "Prenom", model.Prenom },
                    { "Mdp", model.Mdp }
                };

                // Vérification des champs manquants
                var missingFields = requiredFields
                    .Where(field => string.IsNullOrEmpty(field.Value))
                    .Select(field => field.Key)
                    .ToList();

                if (missingFields.Any())
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "Missing required fields",
                        missingFields
                    });
                }

                // Appel de la méthode SInscrire pour inscrire l'utilisateur
                string lienValidation = _inscriptionService.SInscrire(model.Email, model.Nom, model.Prenom, model.Mdp);

                // Retourner le lien de validation à l'utilisateur
                return Ok(new
                {
                    status = "success",
                    message = "Veuillez vérifier votre email pour valider l'inscription.",
                    lienValidation
                });
            }
            catch (Exception ex)
            {
                // En cas d'erreur, retourner une erreur 500 avec le message d'erreur
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Erreur lors de l'inscription : {ex.Message}"
                });
            }
        }

        [HttpPost("validation")]
        [Route("validation")]
        // http://localhost:5032/api/utilisateur/validation
        public IActionResult Validation([FromQuery] int id_inscription,[FromBody] Formulaire form)
        {
            try
            {
                var missingFields = new List<string>();

                // Validation des paramètres d'entrée
                if (form == null)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "Invalid or missing parameters",
                        missingFields = new List<string> { "form" }
                    });
                }

                if (form?.Code == null || form.Code <= 0)
                {
                    missingFields.Add("Code");
                }

                if (missingFields.Count > 0)
                {
                    return BadRequest(new
                    {
                        status = "error",
                        message = "Invalid or missing parameters",
                        missingFields
                    });
                }

                // Accès sécurisé à form.Code
                int code = form.Code ?? 0; // Utilisation d'une valeur par défaut (0) si nécessaire

                // Appel de la méthode de validation
                string result = _inscriptionService.Validation(id_inscription, code);

                // Retourner la réponse
                return Ok(new
                {
                    status = "success",
                    message = "Validation réussie",
                    result
                });
            }
            catch (Exception ex)
            {
                // Gérer les erreurs internes
                return StatusCode(500, new
                {
                    status = "error",
                    message = $"Erreur lors de la validation : {ex.Message}"
                });
            }
        }



    }

}
