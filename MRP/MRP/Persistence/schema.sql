CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    display_name TEXT NULL,
    bio TEXT NULL,
    created TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS media (
    id SERIAL PRIMARY KEY,
    creator_user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title TEXT NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    media_type INT NOT NULL,
    release_year INT NOT NULL,
    genres TEXT[] NOT NULL DEFAULT '{}'::text[],
    age_restriction INT NOT NULL DEFAULT 0,
    avg_score DOUBLE PRECISION NOT NULL DEFAULT 0,
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS ratings (
    id SERIAL PRIMARY KEY,
    media_id INT NOT NULL REFERENCES media(id) ON DELETE CASCADE,
    user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    stars INT NOT NULL,
    comment TEXT NULL,
    confirmed BOOLEAN NOT NULL DEFAULT FALSE,
    created TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NULL,
    CONSTRAINT uq_rating UNIQUE (media_id, user_id)
);

CREATE TABLE IF NOT EXISTS rating_likes (
    rating_id INT NOT NULL REFERENCES ratings(id) ON DELETE CASCADE,
    user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    PRIMARY KEY (rating_id, user_id)
);

CREATE TABLE IF NOT EXISTS favorites (
    user_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    media_id INT NOT NULL REFERENCES media(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, media_id)
);
