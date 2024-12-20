<?php

namespace App\Repository;

use App\Entity\Admin;
use Doctrine\Bundle\DoctrineBundle\Repository\ServiceEntityRepository;
use Doctrine\Persistence\ManagerRegistry;

/**
 * @extends ServiceEntityRepository<Admin>
 */
class AdminRepository extends ServiceEntityRepository
{
    public function __construct(ManagerRegistry $registry)
    {
        parent::__construct($registry, Admin::class);
    }

//    /**
//     * @return Admin[] Returns an array of Admin objects
//     */
//    public function findByExampleField($value): array
//    {
//        return $this->createQueryBuilder('a')
//            ->andWhere('a.exampleField = :val')
//            ->setParameter('val', $value)
//            ->orderBy('a.id', 'ASC')
//            ->setMaxResults(10)
//            ->getQuery()
//            ->getResult()
//        ;
//    }

//    public function findOneBySomeField($value): ?Admin
//    {
//        return $this->createQueryBuilder('a')
//            ->andWhere('a.exampleField = :val')
//            ->setParameter('val', $value)
//            ->getQuery()
//            ->getOneOrNullResult()
//        ;
//    }
    public function save(Admin $entity, bool $flush = false): void
    {
        $entityManager = $this->getEntityManager(); // Correct method to access EntityManager
        $entityManager->persist($entity); // Prepare the entity for saving
        if ($flush) {
            $entityManager->flush(); // Commit changes to the database
        }
    }

    /**
     * Remove an Admin entity.
     */
    public function remove(Admin $entity, bool $flush = false): void
    {
        $entityManager = $this->getEntityManager(); // Correct method to access EntityManager
        $entityManager->remove($entity); // Prepare the entity for deletion
        if ($flush) {
            $entityManager->flush(); // Commit changes to the database
        }
    }
}