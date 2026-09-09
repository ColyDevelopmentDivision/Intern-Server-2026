-- クライアント側の charset を明示的に utf8mb4 に固定する。
-- (Windows の bind mount で /etc/mysql/conf.d が world-writable 扱いになり無視される事故対策)
SET NAMES utf8mb4;

-- 2つのDBを作成し、appuser に両方への権限を付与する
-- docker-compose.yml から MYSQL_DATABASE を外しているため、DB作成もここで行う

CREATE DATABASE IF NOT EXISTS intern_master
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

CREATE DATABASE IF NOT EXISTS intern_user
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

GRANT ALL PRIVILEGES ON intern_master.* TO 'appuser'@'%';
GRANT ALL PRIVILEGES ON intern_user.*   TO 'appuser'@'%';

FLUSH PRIVILEGES;
