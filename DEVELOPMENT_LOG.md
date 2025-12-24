# スイス式トーナメントエディター 開発ログ

## プロジェクト概要

1VS1のマッチ戦で、スイス式トーナメントのマッチングを自動で組むBlazorアプリケーション

### 技術スタック
- .NET 10.0
- Blazor Server
- C#

### 対応仕様
- 参加者数: 2の冪乗（2, 4, 8, 16, 32, 64, 128, 256人）
- ラウンド数: log2(参加者数)で自動計算
- ペアリング方式: 完全スイス式（過去対戦回避）
- 試合結果: 勝敗 + 両負け（両者不戦敗・失格）
- タイブレーク: ソネボーン・ベルガー方式

## 実装した機能

### 1. データモデル (`Models/`)

#### Participant.cs
参加者の情報を管理するモデル
- ID、名前、表示順序
- 勝敗数、獲得ポイント
- ソネボーン・ベルガースコア（タイブレーク用）
- 対戦済み相手のID一覧

#### Match.cs
試合情報を管理するモデル
- ラウンド番号、試合番号
- プレイヤー1、プレイヤー2のID
- 試合結果、完了日時、メモ

#### MatchResult.cs
試合結果の列挙型
- NotPlayed（未実施）
- Player1Win（プレイヤー1勝利）
- Player2Win（プレイヤー2勝利）
- BothLoss（両負け - 両者不戦敗・失格）

#### Tournament.cs
トーナメント全体を管理するモデル
- トーナメント名、作成日時
- 総ラウンド数、現在のラウンド
- 参加者リスト、試合リスト
- 完了フラグ

### 2. ビジネスロジック (`Services/`)

#### SwissPairingService.cs
スイス式トーナメントのコアロジック

**主要メソッド:**
- `CalculateRounds(int participantCount)`: ラウンド数の計算
- `IsPowerOfTwo(int n)`: 2の冪乗判定
- `GeneratePairings(Tournament, int roundNumber)`: 次ラウンドのペアリング生成
  - ポイント順でソート
  - ポイントグループ内で未対戦相手を優先してマッチング
  - 過去対戦履歴を考慮
- `UpdateParticipantStats(Tournament, Match)`: 試合結果の反映
- `CalculateSonnebornBerger(Tournament)`: タイブレークスコアの計算

**ソネボーン・ベルガー方式とは:**
同点の参加者の順位を決定するタイブレーク方式。自分が勝利した対戦相手の獲得ポイント合計を計算する。より強い相手に勝った選手を高く評価する。

#### TournamentService.cs
トーナメント全体の管理サービス（シングルトン）

**主要メソッド:**
- `CreateTournament(string name, List<string> participantNames)`: トーナメント作成
- `StartNextRound()`: 次ラウンド開始
- `RecordMatchResult(Guid matchId, MatchResult result)`: 試合結果記録
- `IsCurrentRoundComplete()`: 現ラウンド完了確認
- `GetStandings()`: 順位表取得
- `IsTournamentComplete()`: トーナメント完了確認
- `CompleteTournament()`: トーナメント完了処理

### 3. ユーザーインターフェース (`Components/Pages/`)

#### Home.razor
ホーム画面
- スイス式トーナメントの説明
- トーナメント作成/管理ページへのリンク

#### TournamentCreate.razor
トーナメント作成画面
- トーナメント名入力
- 参加者数選択（2, 4, 8, 16, 32, 64, 128, 256人）
- 参加者名入力（テキストエリアで1行に1人）
- 入力バリデーション

#### TournamentMain.razor
トーナメント管理画面（メイン）
- トーナメント情報表示
  - 総ラウンド数、現在のラウンド
  - 参加者数、ステータス
- ラウンド制御
  - 第1ラウンド開始ボタン
  - 次のラウンド開始ボタン
  - トーナメント完了ボタン
- 対戦表表示
  - 試合番号、対戦カード
  - 試合結果（未実施/勝利/両負け）
  - 結果入力ボタン（P1勝利/P2勝利/両負け/リセット）
- 順位表表示
  - 順位、参加者名
  - 勝敗数、ポイント
  - ソネボーン・ベルガースコア

#### NavMenu.razor
ナビゲーションメニュー
- ホーム
- トーナメント作成
- トーナメント管理

### 4. 依存性注入設定 (`Program.cs`)

```csharp
builder.Services.AddSingleton<TournamentService>();
```

TournamentServiceをシングルトンとして登録（アプリケーション全体で1つのインスタンスを共有）

## アルゴリズム詳細

### スイス式ペアリングアルゴリズム

1. 全参加者をポイント順にソート（降順）
   - 第1優先: ポイント数
   - 第2優先: ソネボーン・ベルガースコア
   - 第3優先: 表示順序

2. ポイントグループごとに処理
   - 同じポイント数の参加者をグループ化

3. グループ内でペアリング
   - 最上位の参加者を選択
   - 未対戦の相手を優先的に検索
   - 見つからない場合は最も近い相手を選択
   - ペアリング完了後、両者を「ペア済み」としてマーク

4. グループを超えてのペアリング
   - グループ内でペアになれなかった参加者は次のグループと組む

### ソネボーン・ベルガー計算

```
SBスコア = Σ（勝利した対戦相手の獲得ポイント）
```

**例:**
- A選手: 2勝1敗
  - 勝利した相手1: 1pt
  - 勝利した相手2: 3pt
  - SB = 1 + 3 = 4.0

- B選手: 2勝1敗
  - 勝利した相手1: 0pt
  - 勝利した相手2: 1pt
  - SB = 0 + 1 = 1.0

→ A選手が上位（より強い相手に勝っている）

## ファイル構成

```
TournamentEditor/
├── Models/
│   ├── Participant.cs          # 参加者モデル
│   ├── Match.cs                # 試合モデル
│   ├── MatchResult.cs          # 試合結果列挙型
│   └── Tournament.cs           # トーナメントモデル
├── Services/
│   ├── SwissPairingService.cs  # スイス式ペアリングロジック
│   └── TournamentService.cs    # トーナメント管理サービス
├── Components/
│   ├── Pages/
│   │   ├── Home.razor          # ホーム画面
│   │   ├── TournamentCreate.razor  # トーナメント作成画面
│   │   └── TournamentMain.razor    # トーナメント管理画面
│   └── Layout/
│       └── NavMenu.razor       # ナビゲーションメニュー
└── Program.cs                  # エントリーポイント・DI設定
```

## 起動方法

```bash
cd /Users/user/src/projects/tournament-editor/TournamentEditor
dotnet run
```

ブラウザで `https://localhost:XXXX` にアクセス

## 今後の拡張案

### データ永続化
- JSON形式でのエクスポート/インポート
- LocalStorageへの保存
- データベース連携（SQLite, PostgreSQL等）

### 複数トーナメント管理
- トーナメント一覧画面
- トーナメントの切り替え
- 過去のトーナメント履歴

### エクスポート機能
- 対戦表のPDF出力
- CSV形式での結果エクスポート
- 印刷用レイアウト

### 参加者管理機能
- 参加者の追加/削除/編集
- 参加者の途中棄権処理
- シード権の設定

### UI/UX改善
- リアルタイム更新（SignalR）
- モバイル対応の強化
- ダークモード対応
- 多言語対応

### 統計機能
- 対戦履歴の詳細表示
- 参加者間の対戦成績マトリックス
- グラフやチャートでの可視化

## プロジェクト構成のリファクタリング (2025-12-25)

ビジネスロジックを独立したクラスライブラリに分離し、テスタビリティを向上させました。

### 新しいプロジェクト構成

```
tournament-editor/
├── TournamentEditor.Core/          # クラスライブラリ（Blazor非依存）
│   ├── Models/                     # データモデル
│   └── Services/                   # ビジネスロジック
├── TournamentEditor/               # Blazor Webアプリケーション
│   └── Components/                 # UIコンポーネント
└── TournamentEditor.Tests/         # xUnitテストプロジェクト
    ├── SwissPairingServiceTests.cs # ペアリングロジックのテスト
    └── TournamentServiceTests.cs   # トーナメント管理のテスト
```

### テストカバレッジ

**SwissPairingServiceTests (17テスト)**
- ラウンド数計算のバリデーション
- 2の冪乗判定
- 初回ペアリング生成
- 過去対戦相手の回避確認
- 試合結果の反映（勝敗、両負け）
- ソネボーン・ベルガースコア計算

**TournamentServiceTests (25テスト)**
- トーナメント作成のバリデーション
- ラウンド進行管理
- 試合結果記録
- ラウンド完了判定
- 順位表生成
- トーナメント完了処理
- 複数ラウンドの状態管理

### テスト実行結果

```bash
dotnet test
```

**結果: 42テスト全て成功** ✓
- 失敗: 0
- 合格: 42
- スキップ: 0
- 実行時間: 34ms

### 利点

1. **Blazorからの独立**: CoreプロジェクトはBlazorに依存せず、純粋なC#ロジック
2. **テスタビリティ**: Web UIなしで高速なユニットテスト実行が可能
3. **再利用性**: 他のUIフレームワーク（WPF、Console、Web API等）でも利用可能
4. **保守性**: ビジネスロジックとUI層が明確に分離

## 開発日

2025-12-25

## 開発環境

- macOS (Darwin 24.6.0)
- .NET 10.0
- Blazor Server (Interactive Server Components)
- xUnit (テストフレームワーク)
