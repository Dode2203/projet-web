<?php

namespace App\Annotation;

use Attribute;

/**
 * @Annotation
 */
#[Attribute(Attribute::TARGET_METHOD | Attribute::TARGET_CLASS)]
class TokenRequired
{
    private array $roles;

    public function __construct(array $roles = [])
    {
        $this->roles = $roles;
    }

    public function getRoles(): array
    {
        return $this->roles;
    }
}
