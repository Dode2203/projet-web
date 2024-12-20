<?php
namespace App\Repository;

use App\Entity\AProposLocation;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;

/**
 * @method AProposLocation|null find($id, $lockMode = null, $lockVersion = null)
 * @method AProposLocation|null findOneBy(array $criteria, array $orderBy = null)
 * @method AProposLocation[]    findAll()
 * @method AProposLocation[]    findBy(array $criteria, array $orderBy = null, $limit = null, $offset = null)
 */
class AProposLocationRepository extends ServiceEntityRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, AProposLocation::class);
    }

    /**
     * Retourne la somme de `nb` par mois entre deux dates spécifiées (date_debut et date_fin).
     *
     * @param \DateTime $dateDebut
     * @param \DateTime $dateFin
     * @return array Tableau des résultats avec mois, année, et somme de `nb`.
     */
    public function sumNbByMonthRange(\DateTime $dateDebut, \DateTime $dateFin): array
    {
        // Créer un tableau pour stocker les résultats
        $results = [];

        // On commence avec le mois de début
        $currentMonth = $dateDebut;
        $endMonth = $dateFin;

        // Tant que le mois actuel est inférieur ou égal au mois de fin
        while ($currentMonth <= $endMonth) {
            $month = $currentMonth->format('m'); // Mois
            $year = $currentMonth->format('Y');  // Année

            // Utiliser la méthode findBy pour récupérer les résultats correspondant au mois et à l'année
            $records = $this->findBy([
                'mois' => $month,
                'annee' => $year
            ]);

            // Somme des nb pour ce mois et cette année
            $sumLoyer = array_sum(array_map(function($record) {
                try {
                    // Tentative de récupération du loyer
                    return $record->getIdLocationBien()->getIdBien()->getLoyer();
                } catch (\Exception $e) {
                    // Si une exception se produit, retourner 0
                    return 0;
                }
            }, $records));
            $sumGain = array_sum(array_map(function($record) {
                try {
                    // Tentative de récupération du loyer
                    return $record->getIdLocationBien()->getIdBien()->getGain();
                } catch (\Exception $e) {
                    // Si une exception se produit, retourner 0
                    return 0;
                }
            }, $records));
            

            // Ajouter le résultat au tableau
            $results[] = [
                'mois' => (int) $month,
                'annee' => (int) $year,
                'loyer' => (float) $sumLoyer,
                'gain' => (float) $sumGain
            ];

            // Passer au mois suivant
            $currentMonth->modify('first day of next month');
        }

        // Retourner le tableau des résultats
        return $results;
    }
    public function sumNbByMonthRangeProprietaire(\DateTime $dateDebut, \DateTime $dateFin, $idProprietaire): array
    {
        // Créer un tableau pour stocker les résultats
        $results = [];

        // On commence avec le mois de début
        $currentMonth = $dateDebut;
        $endMonth = $dateFin;

        // Tant que le mois actuel est inférieur ou égal au mois de fin
        while ($currentMonth <= $endMonth) {
            $month = $currentMonth->format('m'); // Mois
            $year = $currentMonth->format('Y');  // Année

            // Utiliser la méthode findBy pour récupérer les résultats correspondant au mois, à l'année et au propriétaire
            $records = $this->findBy([
                'mois' => $month,
                'annee' => $year
            ]);

            // Filtrer les enregistrements en fonction de l'id du propriétaire
            $records = array_filter($records, function($record) use ($idProprietaire) {
                $bien = $record->getIdLocationBien();
                // $record->getIdLocationBien()->getIdBien()
                return $bien && $bien->getIdBien()->getProprietaire()->getId() === $idProprietaire;
            });

            // Somme des loyers et gains pour ce mois et ce propriétaire
            $sumLoyer = array_sum(array_map(function($record) {
                try {
                    return $record->getIdLocationBien()->getIdBien()->getLoyer();
                } catch (\Exception $e) {
                    return 0;
                }
            }, $records));

            $sumGain = array_sum(array_map(function($record) {
                try {
                    return $record->getIdLocationBien()->getIdBien()->getGain();
                } catch (\Exception $e) {
                    return 0;
                }
            }, $records));

            // Calcul du propriétaire
            $proprietaire = $sumLoyer - $sumGain;

            // Ajouter le résultat au tableau
            $results[] = [
                'mois' => (int) $month,
                'annee' => (int) $year,
                'loyer' => (float) $sumLoyer,
                'gain' => (float) $sumGain,
                'proprietaire' => (float) $proprietaire,
            ];

            // Passer au mois suivant
            $currentMonth->modify('first day of next month');
        }

        // Retourner le tableau des résultats
        return $results;
    }

}
