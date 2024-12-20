create database fournisseur;
\c fournisseur
drop table inscription;
drop table pin;
drop table utilisateur;
drop table tentative;

CREATE TABLE inscription (
    id_inscription SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    nom VARCHAR(100) NOT NULL,
    prenom VARCHAR(100) NOT NULL,
    mdp VARCHAR(255) NOT NULL,
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE pin(
    id SERIAL PRIMARY KEY,
    id_inscription int,
    code int
);
CREATE TABLE utilisateur (
    id_utilisateur SERIAL PRIMARY KEY,
    email VARCHAR(255) NOT NULL,
    nom VARCHAR(100) NOT NULL,
    prenom VARCHAR(100) NOT NULL,
    mdp VARCHAR(255) NOT NULL,
    date_creation TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
CREATE TABLE tentative(
    id SERIAL PRIMARY KEY,
    id_utilisateur int,
    nombre int
);
