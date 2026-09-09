# Intern-Server-2026

Coly 2026年度 1day インターン（サーバー会）で使う、ガチャ API のサーバーサイド開発テンプレート。

- **API**: ASP.NET Core Web API (.NET 10)
- **DB**: MySQL 8.4 (Docker)
- **DB アクセス**: [Dapper](https://github.com/DapperLib/Dapper) + [MySqlConnector](https://mysqlconnector.net/)
- **API 仕様確認・動作確認**: Swagger UI ([Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore))

MySQL と phpMyAdmin は Docker で動かし、API はローカルの .NET SDK で動かして VSCode からデバッグする構成。**プロジェクトは生成済み**なので、ゼロから作る必要はない。

---

## このリポジトリの読み方

| あなたは | 読むもの |
|---------|---------|
| 参加学生・**事前準備**（PC に Docker / .NET / VSCode を入れる） | [`docs/student/README.md`](./docs/student/README.md) |
| 参加学生・**当日**（課題に取り組む） | [`docs/day/README.md`](./docs/day/README.md) |
| 参加学生・**API を叩いて結果を確かめたい** | [`docs/day/動作確認マニュアル.md`](./docs/day/動作確認マニュアル.md) |

> 📌 この README は **リポジトリの地図** です。課題の内容・ヒント・進め方は `docs/day/` にまとまっているので、ここには書いていない。

---

## 1. クイックスタート

前提: [§03 環境構築 - Windows](./docs/student/03-環境構築-Windows.md) または [§04 環境構築 - macOS](./docs/student/04-環境構築-Mac.md) に沿って **Docker Desktop / .NET 10 SDK / VSCode + C# Dev Kit** が入っていること。

### 1.1 MySQL + phpMyAdmin を起動する

リポジトリのルートで:

```sh
docker compose up -d
```

```sh
docker compose ps
```

`mysql` が `Up (healthy)`、`phpmyadmin` が `Up` になれば OK（初回はイメージ取得と初期化 SQL の実行で数分かかる）。

### 1.2 API を起動する

どちらか一方で:

- **VSCode**: 「実行とデバッグ」ビュー → 上部ドロップダウンで **`.NET Core Launch (InternApi / http)`** を選んで ▶（F5）
- **ターミナル**:
  ```sh
  cd src/InternApi
  dotnet run --launch-profile http
  ```

起動後、ブラウザで <http://localhost:5109/swagger> が開けば疎通完了。

> 💡 スクリーンショット付きの手順、Swagger / phpMyAdmin の操作方法、コード変更後の再起動方法は [`docs/day/動作確認マニュアル.md`](./docs/day/動作確認マニュアル.md) にまとまっている。

---

## 2. URL・接続情報

| 用途 | 値 |
|-----|---|
| Swagger UI | <http://localhost:5109/swagger> |
| OpenAPI 定義 (JSON) | <http://localhost:5109/swagger/v1/swagger.json>（Postman 等にインポート可） |
| phpMyAdmin | <http://localhost:8080>（`appuser` で自動ログイン。ログイン画面は出ない） |
| MySQL（ホストのクライアントから） | Host `127.0.0.1` / Port `3306` / User `appuser` / Password `apppass` / DB `intern_master` または `intern_user` |

起動プロファイルは `http`（`http://localhost:5109`）と `https`（`https://localhost:7002` + `http://localhost:5109`）の2つ。**基本は `http` を使う**。`https` はブラウザ利用専用で、初回に `dotnet dev-certs https --trust` が必要。Unity から叩く場合は開発用証明書を信頼しないため `http` 一択。

---

## 3. リポジトリ構成

```
Intern-Server-2026/
├── docker-compose.yml               # MySQL + phpMyAdmin
├── docker/mysql/
│   ├── conf.d/charset.cnf           # utf8mb4 + JST
│   └── init/                        # コンテナ初回起動時に番号順で実行
│       ├── 01_databases.sql         # intern_master / intern_user 作成 + GRANT
│       ├── 03_user_tables.sql       # ユーザー DB のテーブル + 初期データ
│       └── 04_master_gacha.sql      # マスタ DB のテーブル + 初期データ
├── src/InternApi/
│   ├── Controllers/                 # 1 エンドポイント 1 ファイル
│   │   ├── Gacha/                   # POST /api/gacha/draw
│   │   ├── Users/                   # POST /api/user/create, /api/user/login (参考実装)
│   │   └── Debug/                   # /api/debug/* (Service 単体確認・所持品確認)
│   ├── Services/
│   │   ├── GachaService.cs          # マスタ読み取り・抽選
│   │   ├── UserItemService.cs       # user_items の操作
│   │   └── UserService.cs           # user_profiles の操作 (参考実装)
│   ├── Models/                      # テーブル対応クラス / InParam / OutParam
│   ├── Properties/launchSettings.json   # 起動プロファイル (ポート・環境変数)
│   ├── Program.cs                   # DI 登録 + Swagger
│   ├── appsettings.Development.json # DB 接続文字列 (Master / User)
│   └── InternApi.csproj
├── .vscode/
│   ├── launch.json                  # launchSettings.json のプロファイル名を参照するだけ
│   └── tasks.json                   # build / watch
├── docs/
│   ├── student/                     # 事前準備ガイド
│   └── day/                         # 当日の課題・動作確認マニュアル
└── Intern-Server-2026.sln
```

### 設定はどこで決まるか

| 決めたいこと | 場所 |
|------------|-----|
| ポート・環境変数・ブラウザ自動オープン | `src/InternApi/Properties/launchSettings.json`（VSCode の F5 も `dotnet run` もここを参照する。`.vscode/launch.json` にはハードコードしない） |
| DB 接続文字列 | `src/InternApi/appsettings.Development.json` |
| テーブル定義・初期データ | [`docker/mysql/init/*.sql`](./docker/mysql/init/)（変更後は [環境のリセット](#環境のリセット) で反映） |
| MySQL / phpMyAdmin のポート・パスワード | `docker-compose.yml` |

---

## 4. DB 構成

同一 MySQL インスタンス内に 2 つの DB を立てている。

| DB 名 | 用途 | 主なテーブル |
|------|------|------------|
| `intern_master` | マスタデータ（アイテム・ガチャの定義） | `item_master` / `gacha_master` / `gacha_detail_master` |
| `intern_user` | ユーザーデータ（プロフィール・所持品） | `user_profiles` / `user_items` |

初期データの中身（アイテム一覧・ガチャバナー・サンプルユーザー）は [`docker/mysql/init/`](./docker/mysql/init/) の SQL か、phpMyAdmin で直接見るのが正確。

---

## 5. API エンドポイント一覧

[`docs/day/動作確認マニュアル.md` の「エンドポイント一覧」](./docs/day/動作確認マニュアル.md#エンドポイント一覧) を参照。Swagger UI (<http://localhost:5109/swagger>) を開けば同じ一覧が実物で見られる。

---

## 6. よく使うコマンド

| やりたいこと | コマンド |
|------------|--------|
| MySQL + phpMyAdmin 起動 | `docker compose up -d` |
| コンテナ状態確認 | `docker compose ps` |
| MySQL のログを追う | `docker compose logs -f mysql` |
| MySQL CLI（マスタ DB） | `docker compose exec mysql mysql -uappuser -papppass intern_master` |
| MySQL CLI（ユーザー DB） | `docker compose exec mysql mysql -uappuser -papppass intern_user` |
| API 起動 | `dotnet run --launch-profile http`（`src/InternApi/` で） |
| コード変更を検知して自動再起動 | `dotnet watch run --launch-profile http`（`src/InternApi/` で） |
| ビルドだけ | `dotnet build`（リポジトリルートで） |

### 環境のリセット

DB を初期状態に戻したい時（マスタを書き換えた後、`docker/mysql/init/*.sql` を編集した後など）:

```sh
docker compose down -v
```

```sh
docker compose up -d
```

`-v` で名前付き volume も消えるので、`init/*.sql` が再実行される。**マスタの変更も `user_items` の変更も全部消える**ことに注意。

---

## 7. トラブルシューティング（このリポジトリ固有）

インストールや Hello World 段階のトラブルは [`docs/student/06-トラブルシューティング.md`](./docs/student/06-トラブルシューティング.md) を参照。ここではこのリポジトリを動かす時に起きるものだけ。

| 症状 | 原因 / 対処 |
|-----|-----------|
| `Access denied for user 'appuser'@'...'` | 初期化 SQL や `docker-compose.yml` のパスワードを触った後に volume が古いまま。[環境のリセット](#環境のリセット) で作り直す |
| `Can't connect to MySQL server on '127.0.0.1'` | `docker compose ps` で `mysql` が `healthy` か確認。3306 が別プロセス（ローカル MySQL 等）に取られている場合は `lsof -i :3306`（Mac）/ `netstat -ano \| findstr :3306`（Windows）で確認 |
| ポート `5109` / `7002` が使用中 | 前回の `dotnet run` が残っていないか確認。変える場合は `launchSettings.json` の `applicationUrl` だけ書き換える |
| ポート `8080` が使用中 | `docker-compose.yml` の `phpmyadmin.ports` を `"8081:80"` 等に変更 |
| Swagger UI が 404 / 真っ白 | `ASPNETCORE_ENVIRONMENT` が `Development` 以外になっている（`Program.cs` は開発環境でだけ Swagger を有効化する）。`launchSettings.json` のプロファイル経由で起動しているか確認 |
| phpMyAdmin で `mysqli::real_connect(): (HY000/2002)` | MySQL が `healthy` になる前にアクセスした。数秒待ってリロード |
| F5 でブラウザが自動で開かない | C# Dev Kit のバージョンによる。<http://localhost:5109/swagger> を手で開けばよい |
| Unity から HTTPS で `SSL Handshake Failed` | Unity は .NET の開発用証明書を信頼しない。`http` プロファイルを使う |
| Apple Silicon で `no matching manifest for linux/arm64` | 通常は出ない（MySQL 8.4 は ARM64 対応済み）。出た場合のみ `docker-compose.yml` の `mysql` に `platform: linux/amd64` を追記 |

---

## 参考リンク

- [.NET 10 リリースノート](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview)
- [Dapper](https://github.com/DapperLib/Dapper)
- [MySqlConnector](https://mysqlconnector.net/)
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [MySQL 8.4 リファレンスマニュアル](https://dev.mysql.com/doc/refman/8.4/en/)
