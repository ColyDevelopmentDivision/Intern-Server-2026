-- クライアント側の charset を明示的に utf8mb4 に固定する。
-- (Windows の bind mount で /etc/mysql/conf.d が world-writable 扱いになり無視される事故対策)
SET NAMES utf8mb4;

-- ユーザーDB: プレイヤー個別の状態・進捗

USE intern_user;

CREATE TABLE IF NOT EXISTS user_profiles (
    id         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    name       VARCHAR(64)     NOT NULL,
    level      INT UNSIGNED    NOT NULL DEFAULT 1,
    created_at DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    -- name は Login のキーとして機能させるため UNIQUE。同名ユーザー作成は 409 で拒否する運用。
    UNIQUE KEY uq_user_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- user_items.item_id は intern_master.item_master.item_id を参照する
-- (通貨アイテム: 1001 魔石 / 排出キャラ: 2001-2008)。
-- **MySQLはDB跨ぎのFOREIGN KEYを張れない**ため物理FKなし。
-- 参照整合性はアプリ側の責務。
-- (user_id, item_id) は UNIQUE。同じアイテムは1行にまとめ quantity で加算する運用。
CREATE TABLE IF NOT EXISTS user_items (
    id          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    user_id     BIGINT UNSIGNED NOT NULL,
    item_id     BIGINT UNSIGNED NOT NULL,
    quantity    INT UNSIGNED    NOT NULL DEFAULT 1,
    acquired_at DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_user_item (user_id, item_id),
    INDEX idx_user_id (user_id),
    CONSTRAINT fk_user_items_user
        FOREIGN KEY (user_id) REFERENCES user_profiles(id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO user_profiles (name, level) VALUES
    ('Alice',   99),
    ('Bob',     42),
    ('Charlie',  1);

-- 初期所持品: (user_id, item_id, quantity)
-- item_id 1001: 魔石 (ガチャコスト用, intern_master.item_master)
-- item_id 2001-2008: ガチャ排出品 (intern_master.item_master)
INSERT INTO user_items (user_id, item_id, quantity) VALUES
    (1, 1001, 3000),      -- Alice:   魔石 x3000 (通常ガチャ 10回分)
    (1, 2005,    1),      -- Alice:   森の弓使い x1 (先行所持)
    (2, 1001, 1500),      -- Bob:     魔石 x1500 (通常ガチャ 5回分)
    (3, 1001,  300);      -- Charlie: 魔石 x300  (通常ガチャ 1回分)
