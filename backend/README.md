# VR Multi Math Battle - Backend

Hono on Cloudflare WorkersとD1データベースを使用したVR Math Battleのバックエンドサーバー。

## 技術スタック

- **フレームワーク**: [Hono](https://hono.dev/) - 高速で軽量なWeb フレームワーク
- **ランタイム**: Cloudflare Workers
- **データベース**: Cloudflare D1 (SQLite)
- **ORM**: Drizzle ORM
- **バリデーション**: Zod + @hono/zod-openapi
- **AI**: Google Gemini API（画像認識＆採点）
- **API ドキュメント**: Swagger UI

## 機能

### 実装済み
- ✅ 問題管理 (CRUD)
- ✅ ランダム問題取得
- ✅ AI採点機能（Gemini API統合）
- ✅ OpenAPI / Swagger UI
- ✅ CORS設定

## セットアップ

### 1. 依存関係のインストール

```bash
npm install
```

### 2. 環境変数の設定

`.dev.vars`ファイルにGemini API キーを設定：

```bash
# .dev.vars
GEMINI_API_KEY=your-actual-api-key-here
```

### 3. D1データベースのセットアップ

```bash
# マイグレーションを適用（ローカル）
wrangler d1 migrations apply DB --local

# マイグレーションを適用（本番）
wrangler d1 migrations apply DB --remote
```

### 4. 初期データの投入（オプション）

```bash
# サンプル問題を追加
curl -X POST http://localhost:8787/problems \
  -H "Content-Type: application/json" \
  -d '{
    "question": "2 + 2は？",
    "correctAnswer": "4",
    "difficulty": 1,
    "category": "算数"
  }'
```

## 開発

### 開発サーバーの起動

```bash
bun run dev
```

サーバーは `http://localhost:8787` で起動します。

### Swagger UIでAPIをテスト

ブラウザで `http://localhost:8787/docs` を開いてください。

### 主要エンドポイント

- `GET /problems` - 全問題を取得
- `GET /problems/random` - ランダムな問題を取得
- `POST /problems` - 新規問題を作成
- `POST /grade` - 画像を送信してAI採点

## プロジェクト構造

```
backend/
├── src/
│   ├── database/
│   │   └── schema.ts          # Drizzleスキーマ定義
│   ├── migrations/             # D1マイグレーション
│   ├── routes/
│   │   ├── problems.ts        # 問題管理エンドポイント
│   │   └── grade.ts           # AI採点エンドポイント
│   ├── types/
│   │   ├── problems.ts        # 問題関連のZodスキーマ
│   │   └── grade.ts           # 採点関連のZodスキーマ
│   └── index.ts               # メインエントリーポイント
├── wrangler.jsonc             # Cloudflare Workers設定
├── drizzle.config.ts          # Drizzle ORM設定
├── package.json
└── tsconfig.json
```