# メイン課題1: マスタの確率を反映させる（重み付き抽選）

**目的**: 現在はプール先頭のアイテムを固定で返している `POST /api/gacha/draw` を、マスタ (`gacha_detail_master`) に設定された weight (重み) 通りに抽選するよう実装します。

**目安時間**: 20〜30分

**この課題のゴール**: `POST /api/gacha/draw` を何度も叩いた時、**毎回違うアイテムが出て、SSRは滅多に出ない**状態にすること。

---

## まずは「今の状態」を確認しよう

> 💡 API の叩き方（Swagger の操作、phpMyAdmin での DB 確認など）が分からなくなったら、[動作確認マニュアル](../動作確認マニュアル.md) を参照してください。

`POST /api/gacha/draw` を、以下の Body で叩いてみてください:

```json
{ "gachaId": 1 }
```

**期待するレスポンス (今の状態)**:

```json
{
  "results": [
    { "itemId": 2001, "isNew": true },
    { "itemId": 2001, "isNew": false },
    { "itemId": 2001, "isNew": false },
    { "itemId": 2001, "isNew": false },
    { "itemId": 2001, "isNew": false },
    { "itemId": 2001, "isNew": false },
    { "itemId": 2001, "isNew": false },
    { "itemId": 2001, "isNew": false },
    { "itemId": 2001, "isNew": false },
    { "itemId": 2001, "isNew": false }
  ]
}
```

10連引いても **全部 item_id=2001 (炎の勇者)** が出るはずです。これはまだ「ガチャ」ではなく、「毎回同じ結果を返すだけの仕掛け」の状態です。**これをちゃんと抽選するように書き換える**のが今回の課題です。

---

## ミッション

`src/InternApi/Services/GachaService.cs` の以下のメソッドを書き換えてください:

```csharp
public async Task<int> DrawOneAsync(int gachaId)
{
    var pool = await GetPoolAsync(gachaId);
    if (pool.Count == 0)
        throw new InvalidOperationException($"gacha_id={gachaId} のプールが空。");

    // TODO(メイン課題1): 下の1行を、重み付き抽選ロジックに書き換えよう。
    // 現状は「プール先頭固定」= 何度引いても同じアイテムしか出ない状態。
    return pool[0].ItemId;
}
```

---

## 「重み付き抽選」とは？

通常ガチャ (gachaId=1) のプールはこうです:

| item_id | 名前 | weight |
|---------|------|--------|
| 2001 | 炎の勇者 (SSR) | 1 |
| 2002 | 氷の魔導士 (SSR) | 1 |
| 2003 | 光の聖騎士 (SR) | 8 |
| 2004 | 風の狩人 (SR) | 10 |
| 2005 | 森の弓使い (R) | 30 |
| 2006 | 鉄の戦士 (R) | 25 |
| 2007 | 見習い魔法使い (R) | 15 |
| 2008 | 村人 (R) | 10 |

合計 weight = 100。つまり:
- 「炎の勇者」を引く確率 = 1/100 = **1%**
- 「森の弓使い」を引く確率 = 30/100 = **30%**

**weight は絶対値ではなく相対比率** です。合計が 100 でも 1000 でも動作します。

> 💡 この表と同じデータは **phpMyAdmin** ([http://localhost:8080](http://localhost:8080)) → `intern_master` → `gacha_detail_master` テーブルで確認できます。
> 実装中に「今の設定確認したい」時は、DB を見てみてください

---

<details>
<summary><b>💡 ヒント</b></summary>

### 仕組みを図で見てみよう（数直線イメージ）

weight = [1, 1, 8, 10, 30, 25, 15, 10] を数直線に並べると:

```
0  1  2        10          20                              50                       75              90         100
|--|--|--------|-----------|-------------------------------|-----------------------|---------------|----------|
 2001 2002  2003     2004               2005                          2006                2007          2008
```

`randomNum = _rng.Next(0, 100)` で **0 以上 100 未満**（= 0〜99）の乱数を得ます。

- randomNum=0 → 2001 (最初のブロック)
- randomNum=1 → 2002
- randomNum=50 → 2006 (`0+1+1+8+10+30 = 50` 以上、`50+25 = 75` 未満)
- randomNum=99 → 2008 (最後のブロック)

**先頭から weight を順に足した累積値 `accumulatedWeight` と `randomNum` を比べて、初めて `randomNum < accumulatedWeight` になったブロックが当選** です。

### 実装のコツ

- `_rng.Next(0, totalWeight)` は **0 以上 totalWeight 未満** の乱数を返します（totalWeight ちょうどは含まれません）
- 各アイテムの weight を足しながら、初めて `randomNum < accumulatedWeight` になったところで return

</details>

---

## 動作確認に使えるツール

「マスタで設定した確率通りに抽選できているか」を確認するのに使えるツールです。**具体的にどう使って何をもって「動いた」と判断するかは、自分で考えて実装計画書の「結果確認」に記録**してみてください。

- **Swagger**: `POST /api/gacha/draw`（10連の結果）、`POST /api/debug/gacha-draw-one?gachaId=1`（単発抽選、途中確認に便利）
- **phpMyAdmin**: `intern_master.gacha_detail_master` テーブル（設定した weight を再確認）

> ツールの起動・操作方法は [動作確認マニュアル](../動作確認マニュアル.md) を参照

---

## メイン課題1 完成後の状態

ここまでできると、`POST /api/gacha/draw` はマスタで設定した確率通りに抽選するようになります。ゲーム機能としてはまだ足りない部分もあります:

- **ユーザーIDを受け取らない** → 誰が引いても同じ扱い
- **結果を保存しない** → 何度引いても手元に残りません
- **コストを取らない** → 無限に引けます

> 💡 **この状態は「検証ツール」として使えます**
> ユーザーとの紐付けや所持管理を含まず、抽選ロジックだけを純粋に確認できる状態です。企画者が「マスタで設定した確率通りに当たるか」を試したい時、この API を叩けば分布を確認できるので、**確率調整の検算ツール**として機能します。

**続きは追加課題** で、これを「本物のゲーム機能」に育てていきます。

---

**次に進む**: [メイン課題2: マスタいじりワーク](./2-master-workshop.md)
