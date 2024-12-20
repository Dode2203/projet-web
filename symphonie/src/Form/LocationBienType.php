<?php

namespace App\Form;

use App\Entity\Bien;
use App\Entity\Client;
use App\Entity\LocationBien;
use Symfony\Bridge\Doctrine\Form\Type\EntityType;
use Symfony\Component\Form\AbstractType;
use Symfony\Component\Form\FormBuilderInterface;
use Symfony\Component\OptionsResolver\OptionsResolver;

class LocationBienType extends AbstractType
{
    public function buildForm(FormBuilderInterface $builder, array $options): void
    {
        $builder
            ->add('dateDebut', null, [
                'widget' => 'single_text'
            ])
            ->add('dure')
            ->add('idClient', EntityType::class, [
                'class' => Client::class,
'choice_label' => 'id',
            ])
            ->add('idBien', EntityType::class, [
                'class' => Bien::class,
'choice_label' => 'id',
            ])
        ;
    }

    public function configureOptions(OptionsResolver $resolver): void
    {
        $resolver->setDefaults([
            'data_class' => LocationBien::class,
        ]);
    }
}
