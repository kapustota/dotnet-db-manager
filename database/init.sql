DO $$
BEGIN
   IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'articles_db') THEN
      PERFORM dblink_exec('dbname=postgres', 'CREATE DATABASE articles_db');
   END IF;
END;
$$ LANGUAGE plpgsql;

\c articles_db;

CREATE TABLE articles (
    id SERIAL PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    author VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    annotation TEXT,
    published_date TIMESTAMP NOT NULL
);

INSERT INTO articles (title, author, content, annotation, published_date) VALUES
('First Article', 'Author One', 'Content of the first article', 'Annotation', '2023-01-01 00:00:00'),
('Second Article', 'Author Two', 'Content of the second article', 'Annotation', '2023-02-01 00:00:00');