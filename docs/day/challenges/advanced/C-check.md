# 追加課題C: コスト所持チェック

**目的**: ガチャを引く前に「魔石が足りているか」を確認し、足りなければ 400 エラーで拒否します。

**目安時間**: 10〜15分

**この課題のゴール**: Charlie (魔石300) が通常ガチャ (10連コスト3000) を叩くと **BadRequest** が返る状態です。

**前提**: [メイン課題1](../1-draw.md) + [追加課題A](./A-user-id.md)/[B](./B-grant.md) 完了済み

---

## ミッション

**魔石が足りない時にはガチャを引けないようにしよう**。Charlie (魔石 300) が通常ガチャ (10連 3000 魔石) を叩いたら、抽選が始まる前に 400 で拒否される状態にする。

### 1. 所持数を判定する処理を実装

`src/InternApi/Services/UserItemService.cs` の `HasEnoughAsync` を実装しよう。**「そのユーザーが指定アイテムを requiredCount 個以上持っているか」** を bool で返す。

### 2. Draw の冒頭でチェックを挟む

`GachaDrawController.Draw` で、**抽選を始める前** に (1) で作った処理を使ってコストが足りているかを確認する。足りなければ `BadRequest` を返す。

> 💡 **エラーの返し方 (BadRequest の書き方・メッセージの持たせ方) は `src/InternApi/Controllers/Users/UserCreateController.cs` を参考にできます。** 入力バリデーションで `return BadRequest(new { message = "..." });` の形で 400 を返している既存の実装があります。既存の書き方に揃えることで、他のエンドポイントとレスポンス形式が統一され、クライアント側の扱いも安定します。

> 💡 抽選より**前**にチェックすることが重要です。コスト不足なのに抽選を通してしまうと「引けたのに払えない」矛盾が起きます。追加課題D (消費) と組み合わさった時にも、順序が業務ロジックの正しさを左右します。

---

## ヒント

> 💡 書き方の参考は [ヒント: 参考コード](../../ヒント-参考コード.md) を参照。

`user_items` にそのアイテムの行がない (= まだ何も持っていない) 場合、SELECT の結果は 0 行になります。**「行が無い」を「所持数 0」として扱う** 必要があることを意識しよう。

---

## 動作確認

Swagger で `POST /api/gacha/draw` を叩いて、ユーザーごとの挙動を比べてみよう:

```json
{ "gachaId": 1, "userId": 1 }
```

```json
{ "gachaId": 1, "userId": 3 }
```

- 魔石を十分持っているユーザーと、足りないユーザーで結果はどう変わるか観察しよう

使えるツール:
- `GET /api/debug/item-has-enough?userId=1&itemId=1001&requiredCount=3000` — `HasEnoughAsync` を単体で試したい時に使える
- `GET /api/debug/inventory?userId=3` — そのユーザーの所持一覧を確認
- `POST /api/debug/item-give?userId=3&itemId=1001&grantCount=1200` — Charlie の魔石を増やして成功パターンを試したい時

> ツールの起動・操作方法は [動作確認マニュアル](../../動作確認マニュアル.md) を参照

---

**前に戻る**: [追加課題B: user_items に反映](./B-grant.md)
**次に進む**: [追加課題D: コスト消費](./D-consume.md)
