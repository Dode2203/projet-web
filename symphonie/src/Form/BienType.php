<?php

namespace App\Form;

use App\Entity\Bien;
use App\Entity\Proprietaire;
use App\Entity\TypeBien;
use Symfony\Bridge\Doctrine\Form\Type\EntityType;
use Symfony\Component\Form\AbstractType;
use Symfony\Component\Form\FormBuilderInterface;
use Symfony\Component\OptionsResolver\OptionsResolver;

class BienType extends AbstractType
{
    public function buildForm(FormBuilderInterface $builder, array $options): void
    {
        $builder
            ->add('nom')
            ->add('description')
            ->add('region')
            ->add('loyer')
            ->add('proprietaire', EntityType::class, [
                'class' => Proprietaire::class,
'choice_label' => 'id',
            ])
            ->add('typeBien', EntityType::class, [
                'class' => TypeBien::class,
'choice_label' => 'id',
            ])
        ;
    }

    public function configureOptions(OptionsResolver $resolver): void
    {
        $resolver->setDefaults([
            'data_class' => Bien::class,
        ]);
    }
}
