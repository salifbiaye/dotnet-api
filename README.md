# TodoItems API

API REST sécurisée en ASP.NET Core 9.0 pour la gestion de tâches, avec authentification JWT et autorisation par rôles.

## Preuve de réussite du cours

Cours **"Sécurisez votre application .NET"** sur OpenClassrooms — complété à 100% (3/3 objectifs pédagogiques).

> **Note pour le correcteur :** OpenClassrooms utilise une authentification sans mot de passe (connexion par lien envoyé par email uniquement). Il n'est donc pas possible de partager un accès direct au compte. L'email de confirmation reçu ainsi que la capture de la progression à 100% constituent la preuve de réussite du cours.

![Progression 100% sur OpenClassrooms](API%20web/images/open-classroom.jpeg)

![Email de confirmation OpenClassrooms](API%20web/images/mail-preuve.jpeg)

---

## Démarrage rapide (local — 2 commandes)

**Prérequis :** .NET 9.0 SDK

```bash
cd "API web"
dotnet run
```

C'est tout. Le secret JWT de développement est **pré-configuré** — aucune configuration supplémentaire nécessaire.

L'API est disponible sur :
- HTTP : `http://localhost:5284`
- HTTPS : `https://localhost:7250`
- Swagger UI : `http://localhost:5284/swagger`

---

## Pour le professeur — Comment lancer le projet

### Option 1 : En local (le plus simple)

```bash
git clone <url-du-repo>
cd "API web"
dotnet run
```

Ouvrir `http://localhost:5284/swagger` → l'API est prête.

> Aucune configuration requise en mode développement, le secret JWT est inclus dans `appsettings.Development.json`.

### Option 2 : Via Docker

```bash
git clone <url-du-repo>

# 1. Créer le fichier d'environnement
cp .env.example .env

# 2. Ouvrir .env et définir une clé secrète (min. 32 caractères)
#    JWT_SECRET_KEY=mon-secret-tres-securise-32-caracteres-min

# 3. Lancer
docker-compose up --build
```

Ouvrir `http://localhost:8080/swagger` → l'API est prête.

> **Seule étape obligatoire en Docker :** créer le `.env` depuis `.env.example` et y définir `JWT_SECRET_KEY`.

### Variables d'environnement (fichier `.env`)

Copier `.env.example` vers `.env` et adapter :

```env
JWT_SECRET_KEY=votre-cle-secrete-min-32-caracteres-obligatoire
JWT_EXPIRATION_MINUTES=60
ASPNETCORE_ENVIRONMENT=Development
CORS_ALLOWED_ORIGINS=http://localhost:3000,http://localhost:5173
```

| Variable | Obligatoire | Description |
|----------|-------------|-------------|
| `JWT_SECRET_KEY` | **Oui (Docker)** | Clé de signature des tokens JWT (min. 32 caractères) |
| `JWT_EXPIRATION_MINUTES` | Non | Durée de validité du token en minutes (défaut : 60) |
| `ASPNETCORE_ENVIRONMENT` | Non | Environnement applicatif (défaut : Production) |
| `CORS_ALLOWED_ORIGINS` | Non | Origines autorisées pour les requêtes CORS |

---

## Tester l'API via Swagger (pas à pas)

1. Ouvrir `http://localhost:5284/swagger`

2. **Créer un compte admin** — `POST /api/auth/register` → Try it out :
   ```json
   {
     "username": "admin",
     "password": "Test123!",
     "role": "admin"
   }
   ```
   Réponse attendue : `201 Created`

3. **Se connecter** — `POST /api/auth/login` → Try it out :
   ```json
   {
     "username": "admin",
     "password": "Test123!"
   }
   ```
   Copier la valeur du champ `token` dans la réponse.

4. **S'authentifier** — cliquer sur le bouton **Authorize** (icône cadenas en haut à droite), coller le token, cliquer **Authorize** puis **Close**.

5. **Tester les endpoints** :
   - `GET /api/todoitems` → 200 (liste vide au départ)
   - `POST /api/todoitems` → 201
   - `GET /api/todoitems/{id}` → 200

---

## Tester via curl

```bash
BASE=http://localhost:5284

# Créer un compte admin
curl -X POST $BASE/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Test123!","role":"admin"}'

# Se connecter et récupérer le token
curl -X POST $BASE/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Test123!"}'

# Remplacer <token> par la valeur reçue
TOKEN=<token>

# Lister les tâches
curl -X GET $BASE/api/todoitems \
  -H "Authorization: Bearer $TOKEN"

# Créer une tâche (admin uniquement)
curl -X POST $BASE/api/todoitems \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"name":"Faire les courses","isComplete":false}'

# Modifier une tâche (admin uniquement)
curl -X PUT $BASE/api/todoitems/1 \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"id":1,"name":"Faire les courses","isComplete":true}'

# Supprimer une tâche (admin uniquement)
curl -X DELETE $BASE/api/todoitems/1 \
  -H "Authorization: Bearer $TOKEN"
```

---

## Référence API

### Authentification (aucun token requis)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| POST | `/api/auth/register` | Créer un compte utilisateur |
| POST | `/api/auth/login` | Se connecter et obtenir un token JWT |

### Todo Items (token requis)

| Méthode | Endpoint | Description | Rôle requis |
|---------|----------|-------------|-------------|
| GET | `/api/todoitems` | Lister toutes les tâches | `user` ou `admin` |
| GET | `/api/todoitems/{id}` | Récupérer une tâche | `user` ou `admin` |
| POST | `/api/todoitems` | Créer une tâche | `admin` uniquement |
| PUT | `/api/todoitems/{id}` | Modifier une tâche | `admin` uniquement |
| DELETE | `/api/todoitems/{id}` | Supprimer une tâche | `admin` uniquement |

### Health Checks (aucun token requis)

| Méthode | Endpoint | Description |
|---------|----------|-------------|
| GET | `/health` | Vérification basique (toujours 200) |
| GET | `/ready` | Vérification de disponibilité (vérifie la BDD) |

---

## Rôles

| Rôle | Permissions |
|------|-------------|
| `user` | Lecture seule — `GET` uniquement |
| `admin` | CRUD complet — lecture, création, modification, suppression |

Pour obtenir les droits admin : s'inscrire avec `"role": "admin"`.

---

## Structure du projet

```
.
├── API web/                          # Projet API principal
│   ├── Controllers/                  # Contrôleurs (Auth, TodoItems)
│   ├── Models/                       # Modèles et DTOs
│   ├── Services/                     # Logique métier
│   ├── Middleware/                   # Middlewares personnalisés
│   ├── HealthChecks/                 # Vérifications de santé
│   ├── appsettings.json              # Configuration de base
│   ├── appsettings.Development.json  # Config dev (secret JWT inclus)
│   └── Program.cs                    # Point d'entrée
├── API.Tests/                        # Tests unitaires
├── Dockerfile                        # Image Docker
├── docker-compose.yml                # Configuration Docker Compose
├── .env.example                      # Exemple de configuration (copier vers .env)
└── README.md                         # Ce fichier
```

---

## Dépannage

### L'app ne démarre pas — "JWT_SECRET_KEY is not configured"

Vous êtes en mode Production sans clé JWT. Solutions :
- Lancer en mode Development : `ASPNETCORE_ENVIRONMENT=Development dotnet run`
- Ou définir la variable : `export JWT_SECRET_KEY=votre-cle-32-caracteres-min`

### 401 Unauthorized sur les endpoints protégés

- Header `Authorization` manquant
- Token expiré (par défaut 60 min) — se reconnecter via `/api/auth/login`
- Format incorrect — doit être `Bearer <token>`

### 403 Forbidden sur les opérations d'écriture

Le compte connecté a le rôle `user`. Seul `admin` peut créer, modifier ou supprimer. Créer un compte avec `"role": "admin"`.

### Le container Docker ne démarre pas

Vérifier les logs :
```bash
docker-compose logs api
```
S'assurer que `JWT_SECRET_KEY` est défini dans le fichier `.env`.
