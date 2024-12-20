<?php

namespace App\Form;

use App\Entity\AProposLocation;
use App\Entity\LocationBien;
use Symfony\Bridge\Doctrine\Form\Type\EntityType;
use Symfony\Component\Form\AbstractType;
use Symfony\Component\Form\FormBuilderInterface;
use Symfony\Component\OptionsResolver\OptionsResolver;

class AProposLocationType extends AbstractType
{
    public function buildForm(FormBuilderInterface $builder, array $options): void
    {
        $builder
            ->add('mois')
            ->add('annee')
            ->add('nb')
            ->add('idLocationBien', EntityType::class, [
                'class' => LocationBien::class,
'choice_label' => 'id',
            ])
        ;
    }

    public function configureOptions(OptionsResolver $resolver): void
    {
        $resolver->setDefaults([
            'data_class' => AProposLocation::class,
        ]);
    }
}
