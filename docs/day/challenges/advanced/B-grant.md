# 追加課題B: 抽選結果を user_items に反映する

**目的**: 抽選結果が「一時的なものではなく、ちゃんとユーザーの所持品として残る」ようにします。

**目安時間**: 10〜15分

**この課題のゴール**: `POST /api/gacha/draw` を叩いた後、Alice の所持一覧に新しいアイテムが増えている状態です。

**前提**: [メイン課題1](../1-draw.md) + [追加課題A](./A-user-id.md) 完了済み

---

## ミッション

**抽選結果を `user_items` テーブルに書き込む**ようにしよう。次に同じ Draw を叩いた時、そのユーザーの所持一覧に前回のアイテムが増えている状態にする。

### 1. 所持アイテムを付与する処理を実装

`src/InternApi/Services/UserItemService.cs` の `GrantAsync` を実装しよう。**既にそのアイテムを持っているなら数量を加算、持っていないなら新しく行を作る**。

### 2. 抽選後にその処理を呼ぶ

`src/InternApi/Controllers/Gacha/GachaDrawController.cs` の `Draw` メソッドで、抽選結果ができた後に (1) で作った処理を呼び出す。

---

## ヒント

> 💡 書き方の参考は [ヒント: 参考コード](../../ヒント-参考コード.md) を参照。

### GrantAsync の書き方

「持っていれば加算、持っていなければ挿入」の書き方はいくつかあります:

- **1 文で書く**: MySQL の `INSERT ... ON DUPLICATE KEY UPDATE` を使う方法（`user_items` には `(user_id, item_id)` の UNIQUE 制約が張ってあるので使えます）
- **分岐で書く**: 先に SELECT して、あれば UPDATE / なければ INSERT

やりやすい方で書いてみよう。

---

## 動作確認

Swagger で `POST /api/gacha/draw` を叩いた前後で、そのユーザーの所持一覧がどう変わるか比べてみよう:

```json
{ "gachaId": 1, "userId": 1 }
```

- 抽選で当たったアイテムが Alice の所持一覧に反映されているか確認しよう
- **同じ Draw をもう一度叩いた時**、既に持っているアイテムの数量がどう変わるか観察しよう

使えるツール:
- `GET /api/debug/inventory?userId=1` — そのユーザーの所持一覧を確認できる
- phpMyAdmin — `user_items` テーブルを直接見る
- `POST /api/debug/item-give?userId=1&itemId=2001&grantCount=1` — `GrantAsync` を単体で試したい時に使える

> ツールの起動・操作方法は [動作確認マニュアル](../../動作確認マニュアル.md) を参照

---

**前に戻る**: [追加課題A: userId を受け取る](./A-user-id.md)
**次に進む**: [追加課題C: コスト所持チェック](./C-check.md)
