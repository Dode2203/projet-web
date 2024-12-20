using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography;  // Assurez-vous d'inclure cet espace de noms
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace MyApp.Services
{
    public class JwtTokenManager
    {
        private readonly string _secretKey;
        private readonly SigningCredentials _config;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        public JwtTokenManager()
        {
            string secretKey = "your-secret-key"; 
            _secretKey = secretKey;
            var adjustedKey = AdjustKeySize(secretKey);
             _tokenHandler = new JwtSecurityTokenHandler();

        // Création de la clé secrète avec la taille correcte
            var symmetricKey = new SymmetricSecurityKey(adjustedKey);
            _config = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);
            
        }
        public string CreateToken(Dictionary<string, object> claims, int expirationInSeconds)
        {
            var now = DateTime.UtcNow;

            // Convertir le Dictionary en liste de Claims
            var jwtClaims = BuildClaims(claims);

            // Créer le JWT
            var jwtToken = new JwtSecurityToken(
                issuer: "your-app",             // Émetteur
                audience: "your-client",        // Destinataire
                claims: jwtClaims,              // Claims (utilisateur)
                notBefore: now,                 // Date de début de validité
                expires: now.AddSeconds(expirationInSeconds), // Date d'expiration
                signingCredentials: _config     // Clé de signature
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(jwtToken);
        }

        // Méthode pour convertir Dictionary<string, object> en liste de Claims
        private List<Claim> BuildClaims(Dictionary<string, object> claims)
        {
            var claimList = new List<Claim>();

            foreach (var entry in claims)
            {
                if (entry.Value == null) continue;

                // Si la valeur est une liste (comme une liste de rôles)
                if (entry.Value is IEnumerable<string> roles)
                {
                    // Ajouter chaque rôle comme un claim distinct avec la clé fournie
                    foreach (var role in roles)
                    {
                        claimList.Add(new Claim(entry.Key, role));  // Utilise entry.Key pour la clé dynamique
                    }
                }
                else
                {
                    // Si ce n'est pas une liste, ajouter comme un claim normal
                    claimList.Add(new Claim(entry.Key, entry.Value?.ToString() ?? string.Empty));
                }
            }

            return claimList;
        }


                private IEnumerable<Claim> BuildClaims(Dictionary<string, string> claims)
        {
            var claimList = new List<Claim>();

            foreach (var claim in claims)
            {
                claimList.Add(new Claim(claim.Key, claim.Value));
            }

            return claimList;
        }
        private byte[] AdjustKeySize(string secretKey)
        {
            // Si la clé est trop petite (moins de 16 octets), appliquez un hachage
            if (secretKey.Length < 16)
            {
                using (var sha256 = SHA256.Create())
                {
                    // Retourne un hachage de la clé pour obtenir une taille correcte
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
                }
            }
            // Si la clé est trop grande (plus de 256 bits), tronquez la clé à 256 bits
            else if (secretKey.Length > 32)
            {
                return Encoding.UTF8.GetBytes(secretKey.Substring(0, 32)); // 32 octets = 256 bits
            }
            else
            {
                // Si la clé est déjà entre 16 et 32 octets, l'utilisez telle quelle
                return Encoding.UTF8.GetBytes(secretKey);
            }
        }
        public JwtSecurityToken ParseToken(string tokenString)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Lit le token à partir de la chaîne et le convertit en JwtSecurityToken
                var jwtToken = tokenHandler.ReadJwtToken(tokenString);
                return jwtToken;
            }
            catch (Exception ex)
            {
                // Gérer l'erreur en cas de token invalide
                throw new ArgumentException("Invalid token string", ex);
            }
        }
        public bool ValidateToken(JwtSecurityToken token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                
                // Récupère la clé secrète ajustée
                var symmetricKey = new SymmetricSecurityKey(AdjustKeySize("your-secret-key"));
                
                // Crée les paramètres de validation du token
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "your-app",
                    ValidateAudience = true,
                    ValidAudience = "your-client",
                    ValidateLifetime = true, // Vérification de la date d'expiration
                    IssuerSigningKey = symmetricKey, // Clé utilisée pour la signature
                    ClockSkew = TimeSpan.Zero // Ignorer la différence de temps de 5 minutes par défaut
                };

                // Valide le token en utilisant les paramètres de validation
                var principal = tokenHandler.ValidateToken(token.RawData, tokenValidationParameters, out SecurityToken validatedToken);
                
                return true; // Si la validation a réussi, retourne true
            }
            catch (Exception)
            {
                // Si une exception est levée (ex. token invalide, date d'expiration, etc.)
                return false;
            }
        }
        public string? ExtractTokenFromRequest(HttpRequest request)
        {
            // Récupérer l'en-tête Authorization
            var authHeader = request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader))
            {
                // Rechercher un token avec le format Bearer <token>
                var match = Regex.Match(authHeader, @"Bearer\s+(\S+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value; // Retourne la valeur du token
                }
            }

            return null; // Retourne null si aucun token valide n'est trouvé
        }
        public SymmetricSecurityKey GenerateValidKey(string secretKey)
        {
            // Transforme la clé en un hachage SHA256 (256 bits)
            using (var sha256 = SHA256.Create())
            {
                var keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(secretKey));
                return new SymmetricSecurityKey(keyBytes);
            }
        }

        // Exemple d'utilisation
        public IDictionary<string, object>? ExtractClaimsFromToken(string tokenString)
        {
            try
            {
                
                var symmetricKey = GenerateValidKey(_secretKey); // Normaliser la clé

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = symmetricKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();

                var principal = tokenHandler.ValidateToken(tokenString, validationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken)
                    return null;

                var claims = jwtToken.Claims.ToDictionary(claim => claim.Type, claim => (object)claim.Value);
                return claims;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return null;
            }
        }




        
            
    }

}
