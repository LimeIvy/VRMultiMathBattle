# Unity側セットアップガイド

## 📖 概要

このガイドでは、VR Math BattleのUnity側の実装について説明します。バックエンドAPI（Hono on Cloudflare Workers）と通信し、数学問題の取得、黒板の描画内容のキャプチャ、AI採点機能を実装します。

## 🎯 アーキテクチャの理解

### システム全体の流れ

```
Unity (VR空間)          →  Backend API          →  Gemini AI
1. 問題を取得          →  GET /problems/random  →  -
2. 黒板に答えを描画    →  -                     →  -
3. 画像をキャプチャ    →  -                     →  -
4. 採点を依頼          →  POST /grade           →  画像認識＆採点
5. 結果を表示          ←  レスポンス            ←  採点結果
```

### なぜこの構成なのか？

- **バックエンド分離**: Unity単体ではAI機能が使えないため、外部APIを使用
- **Gemini API**: 画像認識に優れており、手書きの数式を正確に認識可能
- **REST API**: 標準的な通信方式で、将来的な拡張が容易

## 📁 作成されたスクリプト

### 1. ApiService.cs - HTTP通信クライアント
**役割**: バックエンドAPIとの全ての通信を担当
**なぜ必要？**: 
- UnityのHTTP通信（UnityWebRequest）を扱いやすくラップ
- エラーハンドリングを統一
- シングルトンパターンでどこからでもアクセス可能

### 2. ProblemManager.cs - 問題管理
**役割**: 数学問題の取得・管理・UI表示
**なぜ必要？**:
- APIから取得した問題データを保持
- 全プレイヤーで同じ問題を共有する基盤
- UIの更新ロジックを一元管理

### 3. BlackboardCapture.cs - 画像キャプチャ＆採点
**役割**: 黒板をキャプチャし、AI採点を依頼
**なぜ必要？**:
- VR空間の3D描画を2D画像に変換（AIが解析可能な形式）
- Base64エンコードでAPIに送信
- 採点結果の受信と表示

## 🎮 セットアップ手順

### 1. Newtonsoft.Jsonパッケージのインストール

#### なぜ必要？
- UnityのデフォルトJSON機能（JsonUtility）は複雑なデータ構造に対応していない
- Newtonsoft.Jsonは業界標準で、高度なシリアライゼーションに対応
- バックエンドとのJSON通信に必須

#### インストール方法
Unity Editorで：
1. `Window` > `Package Manager` を開く
2. `+` ボタン > `Add package from git URL...`
3. `com.unity.nuget.newtonsoft-json` を入力
4. **代替方法**: `Project Settings` > `Player` > `Other Settings` > `Scripting Define Symbols` に `NEWTONSOFT_JSON` を追加

#### 確認方法
- エラーなくコンパイルできればOK
- `using Newtonsoft.Json;` でエラーが出なければ成功

### 2. シーンにコンポーネントを配置

#### A. ApiServiceの設定

**なぜこれが必要？**
- シングルトンパターンで全シーンから通信機能にアクセス
- DontDestroyOnLoadでシーン遷移時も生き残る
- API通信の一元管理で保守性向上

**設定手順:**
1. Hierarchyで右クリック > `Create Empty`
2. GameObject名を `ApiService` に変更
3. `ApiService`コンポーネントを追加
4. **Base URL**: `http://localhost:8787`
   - ローカルテスト用のURL
   - 本番環境では変更が必要

**設定のポイント:**
- Base URLはInspectorから変更可能
- 将来的にCloudflare Workersにデプロイしたら、そのURLに変更

#### B. ProblemManagerの設定

**なぜこれが必要？**
- 現在の問題データを保持（全プレイヤーで同期する基盤）
- UI更新ロジックの一元化
- 問題の状態管理

**設定手順:**
1. Hierarchyで右クリック > `Create Empty`
2. GameObject名を `ProblemManager` に変更
3. `ProblemManager`コンポーネントを追加
4. **UI References を設定:**
   - **Problem Text**: 問題文表示用（例: "2 + 2は？"）
   - **Difficulty Text**: 難易度表示用（例: "難易度: 1"）
   - **Category Text**: カテゴリ表示用（例: "カテゴリ: 算数"）
   - **New Problem Button**: 新しい問題を取得するボタン
5. **Settings:**
   - **Auto Load On Start**: `true` にすると起動時に自動で問題を読み込む

**なぜUIを分離？**
- 3D空間のCanvasやWorldSpaceUIに対応可能
- VR環境では通常のUIと異なる配置が必要
- 柔軟性のため参照で接続

#### C. BlackboardCaptureの設定

**なぜこれが必要？**
- VR空間の3D描画を2D画像に変換（Gemini AIは2D画像を解析）
- 黒板の特定レイヤーのみをキャプチャ（背景やプレイヤーを除外）
- Base64エンコードでJSON形式のAPIに送信可能

**設定手順:**
1. 黒板オブジェクトに`BlackboardCapture`コンポーネントを追加
2. **Capture Settings:**
   - **Capture Camera**: 専用カメラ（後述）
   - **Capture Width/Height**: `1024x1024`
     - なぜ1024？ AIの画像認識に十分な解像度で、ファイルサイズも適切
     - 4MB制限を考慮した最適サイズ
   - **Capture Layer**: 黒板専用レイヤー
     - なぜレイヤー分け？ 描画内容のみをキャプチャし、余計なオブジェクトを除外
3. **UI References:**
   - **Submit Button**: 採点を送信するボタン
   - **Preview Image**: キャプチャ画像のプレビュー（デバッグ用、オプション）
4. **Dependencies:**
   - **Problem Manager**: 現在の問題IDを取得するため必要

### 3. キャプチャカメラの設定

**なぜ専用カメラが必要？**
- メインカメラはプレイヤーの視点で動く
- 黒板を常に正面から撮影する固定カメラが必要
- レイヤーベースで黒板の描画のみをキャプチャ

**カメラ作成手順:**
1. `GameObject` > `Camera` で新規カメラ作成
2. 名前を `BlackboardCaptureCamera` に変更
3. カメラを黒板の正面に配置（重要！）
   - 黒板全体が収まる位置
   - 真正面から撮影（斜めだとAIの認識精度が下がる）

**カメラ設定の詳細:**
- **Projection: Orthographic（推奨）**
  - なぜ？ パースペクティブ（遠近感）がないため、手書き文字が歪まない
  - 遠近感が必要ない黒板キャプチャに最適
- **Target Display: Display 1**
  - 通常のディスプレイ設定でOK
- **Culling Mask: 黒板のレイヤーのみ**
  - なぜ？ 黒板の描画内容だけをキャプチャし、プレイヤーや背景を除外
  - AIの認識精度向上のため
- **Enabled: false（通常時は無効）**
  - なぜ？ キャプチャ時のみ一時的に有効化
  - パフォーマンスの最適化

5. `BlackboardCapture`の`Capture Camera`にこのカメラを設定

### 4. UIの設定

**なぜUIが必要？**
- VR環境でも問題を確認できるようにする
- ボタンで直感的に操作
- フィードバック表示で採点結果を確認

#### 問題表示UI
**目的**: プレイヤーに現在の問題を表示

Canvas上に以下を配置：
```
Canvas (World Space推奨 - VR環境のため)
├── ProblemPanel
│   ├── QuestionText (TextMeshProUGUI) - 問題文
│   ├── DifficultyText (TextMeshProUGUI) - 難易度
│   ├── CategoryText (TextMeshProUGUI) - カテゴリ
│   └── NewProblemButton (Button) - 新しい問題を取得
```

**VR環境での配置のコツ:**
- World Space Canvasを使用（Screen Spaceは非推奨）
- プレイヤーの視線の高さに配置
- 黒板の近くに配置すると便利

#### 採点UI
**目的**: 採点を送信し、結果を確認

Canvas上に以下を配置：
```
Canvas
├── GradingPanel
│   ├── SubmitButton (Button) - 答えを送信
│   └── PreviewImage (RawImage) - キャプチャプレビュー（デバッグ用、オプション）
```

**PreviewImageの用途:**
- 開発中にキャプチャが正しく撮れているか確認
- プレイヤーが送信前に内容を確認
- 本番環境では非表示でもOK

### 5. レイヤー設定

**なぜレイヤーが必要？**
- キャプチャカメラが黒板の描画内容のみを撮影するため
- 背景、プレイヤー、UIなどを除外
- AIの認識精度を上げるため（余計なものが映らない）

**設定手順:**
1. `Edit` > `Project Settings` > `Tags and Layers`
2. 空いているUser Layerに `Blackboard` を追加
3. 以下のオブジェクトをこのレイヤーに設定：
   - 黒板本体
   - ペンの軌跡（描画されたトレイル）
   - 消しゴムで消されていない全ての描画

**重要:**
- プレイヤーやペン本体は**含めない**
- 描画された結果のみをキャプチャ

## 🧪 テスト手順

### 前提条件の確認
以下が必要です：
- Node.js/Bun がインストール済み
- Wrangler CLI がインストール済み (`npm install -g wrangler`)
- Gemini API キーが `.dev.vars` に設定済み

### 1. バックエンドの起動

**なぜ必要？**: Unityから問題を取得・採点を依頼するため

```bash
cd backend
bun run dev
```

**確認方法:**
- ターミナルに `Ready on http://localhost:8787` と表示される
- ブラウザで `http://localhost:8787/hello` にアクセス → `{"message":"Hello Hono!"}` が表示される

### 2. マイグレーション適用（初回のみ）

**なぜ必要？**: データベースにテーブルを作成するため

```bash
wrangler d1 migrations apply DB --local
```

**何が起こる？**
- `problems` テーブル作成（問題データ保存用）
- `grades` テーブル作成（採点履歴保存用）

**確認方法:**
- エラーなく完了すればOK
- `Migrations applied successfully!` のようなメッセージが表示される

### 3. 初期問題の投入

**なぜ必要？**: テスト用の問題データが必要

**方法1: Swagger UI（推奨）**
1. ブラウザで `http://localhost:8787/docs` を開く
2. `POST /problems` を展開
3. `Try it out` をクリック
4. 以下のJSONを入力：
```json
{
  "question": "2 + 2は？",
  "correctAnswer": "4",
  "difficulty": 1,
  "category": "算数"
}
```
5. `Execute` をクリック

**方法2: curl コマンド**
```bash
curl -X POST http://localhost:8787/problems \
  -H "Content-Type: application/json" \
  -d '{
    "question": "3 × 5は？",
    "correctAnswer": "15",
    "difficulty": 2,
    "category": "算数"
  }'
```

**複数問題を追加すると良い理由:**
- ランダム問題取得のテストができる
- 様々な難易度で動作確認

### 4. Unityでテスト

**テストの流れ:**

1. **Unity Editorで再生**
   - Play ボタンをクリック

2. **問題が自動で読み込まれることを確認**
   - Consoleに `問題を読み込みました: 2 + 2は？` と表示される
   - UIに問題文が表示される
   - **失敗する場合**: ApiServiceのBase URLを確認

3. **黒板に答えを描画**
   - VRコントローラーまたはマウスで黒板に「4」と描く
   - 描画が正しく表示されることを確認

4. **送信ボタンをクリック**
   - Submit Button をクリック
   - Consoleに `画像サイズ: XXX KB` と表示される

5. **採点結果を確認**
   - Consoleに採点結果が表示される：
     ```
     採点結果: 正解
     フィードバック: 正解です！計算が正確でした。
     ```

**トラブルシューティング:**
- 問題が読み込めない → バックエンドが起動しているか確認
- 画像が送信されない → Capture Cameraの設定を確認
- 採点結果が返らない → Gemini API キーが正しく設定されているか確認

## 📝 使い方

### 問題の取得
```csharp
// ランダムな問題を取得
problemManager.LoadRandomProblem();

// 現在の問題IDを取得
int problemId = problemManager.GetCurrentProblemId();
```

### 画像のキャプチャと送信
```csharp
// BlackboardCaptureコンポーネントが自動で処理
// SubmitButtonをクリックするだけ
```

### 黒板のクリア
```csharp
blackboardCapture.ClearBlackboard();
```

## 🎨 カスタマイズ

### APIのベースURLを変更
```csharp
// ApiServiceコンポーネントのInspectorで設定
// または、スクリプトから：
ApiService.Instance.baseUrl = "https://your-api.com";
```

### キャプチャ画質の調整
```csharp
// BlackboardCapture.cs の 87行目:
byte[] imageBytes = capturedTexture.EncodeToJPG(75); // 75を変更（0-100）
```

### フィードバック表示のカスタマイズ
`BlackboardCapture.cs`の`ShowFeedback()`メソッドを編集：
- 3D Text表示
- パーティクルエフェクト
- サウンド再生
- UI通知

## 🐛 トラブルシューティング

### 問題が読み込めない
- バックエンドが起動しているか確認
- Console で詳細なエラーメッセージを確認
- `http://localhost:8787/problems/random` にブラウザでアクセスして動作確認

### 画像が送信されない
- Capture Cameraが設定されているか確認
- 黒板のレイヤーが正しく設定されているか確認
- コンソールで画像サイズを確認（4MB以下推奨）

### Newtonsoft.Jsonエラー
```
The type or namespace name 'Newtonsoft' could not be found
```
→ Package Managerで `com.unity.nuget.newtonsoft-json` をインストール

## 🚀 次のステップ

1. **ネットワーク同期の実装**
   - 問題の全クライアント同期
   - 採点結果の共有
   - マルチプレイヤー対応

2. **UI/UXの改善**
   - フィードバックの3D表示
   - スコアボードの実装
   - タイマー機能

3. **ゲームロジックの追加**
   - 勝利条件の実装
   - ポイントシステム
   - ラウンド管理

4. **エフェクトの追加**
   - 正解/不正解のエフェクト
   - サウンド
   - パーティクル
