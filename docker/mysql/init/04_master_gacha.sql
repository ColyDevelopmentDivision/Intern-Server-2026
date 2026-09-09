-- クライアント側の charset を明示的に utf8mb4 に固定する。
-- (Windows の bind mount で /etc/mysql/conf.d が world-writable 扱いになり無視される事故対策)
SET NAMES utf8mb4;

-- マスターDB: ガチャ関連マスタ
-- Excel『1day_Intern_2026_企画たたき_ガチャ』の「マスタ」タブ準拠。
-- クライアントとサーバーで同じ item / rarity ラインナップを共有するための起点。

USE intern_master;

-- ------------------------------------------------------------
-- item_master
--   ガチャの排出アイテムおよびコストアイテムを表す。
--   rarity は "SSR"/"SR"/"R" などの文字列で、クライアントの演出出し分けと直結する。
--   物理FKは張らない (gacha_detail_master 側でも参照)。
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS item_master (
    item_id   INT UNSIGNED    NOT NULL,
    item_type VARCHAR(32)     NOT NULL,
    name      VARCHAR(64)     NOT NULL,
    rarity    VARCHAR(8)      NOT NULL,
    PRIMARY KEY (item_id),
    INDEX idx_rarity (rarity),
    INDEX idx_type   (item_type)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 通貨系 (ガチャコスト用)
INSERT INTO item_master (item_id, item_type, name, rarity) VALUES
    (1001, 'currency', '魔石', '-');

-- キャラクター (排出対象)
INSERT INTO item_master (item_id, item_type, name, rarity) VALUES
    (2001, 'character', '炎の勇者',       'SSR'),
    (2002, 'character', '氷の魔導士',     'SSR'),
    (2003, 'character', '光の聖騎士',     'SR'),
    (2004, 'character', '風の狩人',       'SR'),
    (2005, 'character', '森の弓使い',     'R'),
    (2006, 'character', '鉄の戦士',       'R'),
    (2007, 'character', '見習い魔法使い', 'R'),
    (2008, 'character', '村人',           'R');

-- ------------------------------------------------------------
-- gacha_master
--   ガチャバナー1本の定義。
--   start_at / end_at は仕様上保持するが、サーバー側の期間チェックは基本課題スコープ外
--   (発展課題として活用可能)。
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS gacha_master (
    gacha_id     INT UNSIGNED    NOT NULL,
    name         VARCHAR(64)     NOT NULL,
    start_at     DATETIME        NOT NULL,
    end_at       DATETIME        NOT NULL,
    cost_item_id INT UNSIGNED    NOT NULL,
    cost_amount  INT UNSIGNED    NOT NULL,
    PRIMARY KEY (gacha_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO gacha_master (gacha_id, name, start_at, end_at, cost_item_id, cost_amount) VALUES
    (1, '通常ガチャ',       '2026-01-01 00:00:00', '2099-12-31 23:59:59', 1001, 300),
    (2, '初心者応援ガチャ', '2026-01-01 00:00:00', '2099-12-31 23:59:59', 1001, 150);

-- ------------------------------------------------------------
-- gacha_detail_master
--   ガチャ1本の排出プール。weight は絶対値ではなく相対比率でOK。
--   合計値は任意 (SUM(weight) の中での比率で抽選する設計)。
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS gacha_detail_master (
    gacha_id INT UNSIGNED NOT NULL,
    item_id  INT UNSIGNED NOT NULL,
    weight   INT UNSIGNED NOT NULL,
    PRIMARY KEY (gacha_id, item_id),
    INDEX idx_gacha (gacha_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- gacha_id=1 通常ガチャ (SSR合計2 / SR合計18 / R合計80 の想定)
INSERT INTO gacha_detail_master (gacha_id, item_id, weight) VALUES
    (1, 2001,  1),
    (1, 2002,  1),
    (1, 2003,  8),
    (1, 2004, 10),
    (1, 2005, 30),
    (1, 2006, 25),
    (1, 2007, 15),
    (1, 2008, 10);

-- gacha_id=2 初心者応援ガチャ (SSR合計6 / SR合計29 / R合計65 の想定)
INSERT INTO gacha_detail_master (gacha_id, item_id, weight) VALUES
    (2, 2001,  3),
    (2, 2002,  3),
    (2, 2003, 12),
    (2, 2004, 17),
    (2, 2005, 25),
    (2, 2006, 20),
    (2, 2007, 12),
    (2, 2008,  8);
