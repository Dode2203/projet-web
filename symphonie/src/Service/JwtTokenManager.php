<?php

namespace App\Service;
use Symfony\Component\VarDumper\VarDumper;
use Lcobucci\JWT\Configuration;
use Lcobucci\JWT\Signer\Hmac\Sha256;
use Lcobucci\JWT\Signer\Key\InMemory;
use Lcobucci\JWT\Token;
use Lcobucci\JWT\Validation\Constraint\SignedWith;
use Lcobucci\JWT\Validation\Constraint\ValidAt;
use Lcobucci\Clock\SystemClock;
use Symfony\Component\HttpFoundation\Request;

class JwtTokenManager 
{
    private $config;
    

    public function __construct(string $secretKey)
    {
        $this->config = Configuration::forSymmetricSigner(
            new Sha256(),
            InMemory::plainText($secretKey)
        );
    }

    public function createToken(array $claims, int $expirationInSeconds): Token
    {
        $now = new \DateTimeImmutable('@' . time()); // Convertir le timestamp actuel en DateTimeImmutable
        $expiration = $now->modify("+$expirationInSeconds seconds"); // Ajouter la durée d'expiration
    
        // Construction du token
        $builder = $this->config->builder()
            ->issuedBy('your-app') // Issuer (émetteur)
            ->permittedFor('your-client') // Audience (destinataire)
            ->issuedAt($now) // Date d'émission
            ->expiresAt($expiration); // Date d'expiration
    
        // Ajouter des claims personnalisés
        foreach ($claims as $key => $value) {
            $builder->withClaim($key, $value);
        }
    
        // Retourner le token signé
        return $builder->getToken($this->config->signer(), $this->config->signingKey());
        }


    // public function validateToken(Token $token): bool
    // {
    //     $clock = new SystemClock(new \DateTimeZone('UTC'));

    //     $constraints = [
    //         new SignedWith($this->config->signer(), $this->config->signingKey()),
    //         new ValidAt($clock)
    //     ];

    //     return $this->config->validator()->validate($token, ...$constraints);
    // }
    
    public function validateToken(Token $token): bool
    {
        $clock = new SystemClock(new \DateTimeZone('UTC'));
        $constraints = [
            new SignedWith($this->config->signer(), $this->config->signingKey()),
            new ValidAt($clock, new \DateInterval('PT0S')) // Leeway de 0 secondes
        ];

        // Vérification des claims 'nbf' et 'exp'
        $now = new \DateTimeImmutable('now', new \DateTimeZone('UTC'));
        $nbf = $token->claims()->get('nbf');
        if (isset($nbf)) {
            // Utiliser $nbf s'il est défini
        } else {
            $iat = $token->claims()->get('iat');
            if (isset($iat)) {
                $nbf = $iat; // Utiliser $iat si $nbf n'est pas défini
            }
        }

      
        $exp = $token->claims()->get('exp');


        if ($now < $nbf) {
            
            return false; // Le token n'est pas encore valide
        } elseif ($now > $exp) {
            
            return false; // Le token est expiré
        }

        // Déboguer la validation de la signature
        // if (!$this->config->validator()->validate($token, ...$constraints)) {
        //     echo "Token validation failed due to constraints.\n";
        //     return false;
        // }

        return true;
    }
    public function parseToken(string $tokenString): ?Token
    {
        try {
            return $this->config->parser()->parse($tokenString);
        } catch (\Throwable $e) {
            return null;
        }
    }

    // Method to extract token from the Authorization header
    public function extractTokenFromRequest(Request $request): ?string
    {
        $authHeader = $request->headers->get('Authorization');
        if ($authHeader && preg_match('/Bearer\s(\S+)/', $authHeader, $matches)) {
            return $matches[1];
        }
        return null;
    }
    public function extractClaimsFromToken(string $tokenString): ?array
    {
        // Parse the token
        $token = $this->parseToken($tokenString);
        if ($token === null) {
            return null; // Token invalide ou non parsable
        }

        // Vérifiez la validité du token
        if (!$this->validateToken($token)) {
            return null; // Token non valide
        }

        // Extraire les revendications (claims) du token
        $claims = $token->claims()->all();

        return $claims;
    }

}
