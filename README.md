# スイス式トーナメントエディター

1VS1のマッチ戦で、スイス式トーナメントのマッチングを自動で組むBlazor Webアプリケーション

## 機能概要

- **スイス式トーナメント管理**: 完全スイス式（過去対戦回避）によるペアリング
- **参加者管理**: 2の冪乗（2, 4, 8, 16, 32, 64, 128, 256人）に対応
- **試合結果記録**: 勝敗および両負け（両者不戦敗・失格）に対応
- **順位表生成**: ポイント順＋ソネボーン・ベルガー方式のタイブレーク
- **リアルタイム更新**: Blazor Serverによるインタラクティブな操作

## システム要件

### 実装済み仕様

#### 参加者
- 人数: 2の冪乗（2, 4, 8, 16, 32, 64, 128, 256人）
- バリデーション: 2の冪乗以外はエラー

#### ラウンド
- ラウンド数: log2(参加者数)で自動計算
  - 例: 8人 → 3ラウンド、16人 → 4ラウンド、32人 → 5ラウンド

#### ペアリング方式
- **完全スイス式（過去対戦回避）**
  - 同じポイント数の参加者をグループ化
  - グループ内で未対戦の相手を優先的にマッチング
  - 過去に対戦した相手との再戦を可能な限り回避

#### 試合結果
- **勝ち**: 1ポイント
- **負け**: 0ポイント
- **両負け**: 両者0ポイント（両者不戦敗・失格）

#### タイブレーク
- **ソネボーン・ベルガー方式**
  - 計算式: 自分が勝利した対戦相手の獲得ポイント合計
  - より強い相手に勝った選手を高く評価

#### 順位決定ルール
1. 獲得ポイント（降順）
2. ソネボーン・ベルガースコア（降順）
3. 表示順序（昇順）

## 技術スタック

### フロントエンド
- **Blazor Server** (.NET 10.0)
- **Bootstrap 5** (レスポンシブUI)
- **Interactive Server Components**

### バックエンド
- **.NET 10.0**
- **C# 13**

### テスト
- **xUnit v3.2.1** (Apache 2.0 License)
- **カバレッジ**: 42テスト（全て成功）

## プロジェクト構成

```
tournament-editor/
├── TournamentEditor/               # Blazor Webアプリケーション
│   ├── Components/
│   │   ├── Pages/
│   │   │   ├── Home.razor         # ホーム画面
│   │   │   ├── TournamentCreate.razor  # トーナメント作成
│   │   │   └── TournamentMain.razor    # トーナメント管理
│   │   └── Layout/
│   │       └── NavMenu.razor      # ナビゲーション
│   └── Program.cs                 # エントリーポイント
│
├── TournamentEditor.Core/          # クラスライブラリ（Blazor非依存）
│   ├── Models/
│   │   ├── Participant.cs         # 参加者モデル
│   │   ├── Match.cs               # 試合モデル
│   │   ├── MatchResult.cs         # 試合結果（列挙型）
│   │   └── Tournament.cs          # トーナメントモデル
│   └── Services/
│       ├── SwissPairingService.cs # スイス式ペアリングロジック
│       └── TournamentService.cs   # トーナメント管理サービス
│
└── TournamentEditor.Tests/         # xUnitテストプロジェクト
    ├── SwissPairingServiceTests.cs # ペアリングロジックのテスト (17テスト)
    └── TournamentServiceTests.cs   # トーナメント管理のテスト (25テスト)
```

## セットアップ

### 必要な環境
- .NET 10.0 SDK
- 対応OS: Windows, macOS, Linux

### インストール

```bash
# リポジトリのクローン
git clone <repository-url>
cd tournament-editor

# 依存関係の復元
dotnet restore

# ビルド
dotnet build

# テストの実行
dotnet test

# アプリケーションの起動
cd TournamentEditor
dotnet run
```

アプリケーションが起動したら、ブラウザで `https://localhost:5001` にアクセスしてください。

## 使い方

### 1. トーナメント作成
1. ホーム画面で「新しいトーナメントを作成」をクリック
2. トーナメント名を入力
3. 参加者数を選択（2, 4, 8, 16, 32, 64, 128, 256人）
4. 参加者名を1行に1人ずつ入力
5. 「トーナメント作成」をクリック

### 2. ラウンド進行
1. トーナメント管理画面で「第1ラウンド開始」をクリック
2. 自動生成された対戦表を確認
3. 各試合の結果を記録（P1勝利/P2勝利/両負け）
4. 全試合終了後、「次のラウンド開始」をクリック
5. 全ラウンド完了まで繰り返し

### 3. 順位確認
- リアルタイムで更新される順位表を確認
- 各参加者の勝敗数、ポイント、SBスコアを表示

## データモデル

### Participant（参加者）
```csharp
- Id: Guid
- Name: string
- DisplayOrder: int
- IsActive: bool
- Wins: int
- Losses: int
- Points: decimal
- SonnebornBerger: decimal
- OpponentIds: List<Guid>
```

### Match（試合）
```csharp
- Id: Guid
- RoundNumber: int
- MatchNumber: int
- Player1Id: Guid?
- Player2Id: Guid?
- Result: MatchResult
- Notes: string?
- CompletedAt: DateTime?
```

### MatchResult（試合結果）
```csharp
enum MatchResult
{
    NotPlayed,   // 未実施
    Player1Win,  // プレイヤー1勝利
    Player2Win,  // プレイヤー2勝利
    BothLoss     // 両負け
}
```

### Tournament（トーナメント）
```csharp
- Id: Guid
- Name: string
- CreatedAt: DateTime
- TotalRounds: int
- CurrentRound: int
- Participants: List<Participant>
- Matches: List<Match>
- IsComplete: bool
```

## アルゴリズム詳細

### スイス式ペアリングアルゴリズム

1. **参加者のソート**
   - 第1優先: ポイント数（降順）
   - 第2優先: ソネボーン・ベルガースコア（降順）
   - 第3優先: 表示順序（昇順）

2. **ポイントグループ化**
   - 同じポイント数の参加者をグループ化

3. **グループ内ペアリング**
   - 最上位の未ペア参加者を選択
   - 過去に対戦していない相手を検索
   - 見つからない場合は最も近い相手を選択

4. **グループ間調整**
   - グループ内でペアになれなかった参加者を次のグループと組む

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

## テスト

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

### テスト実行

```bash
# 全テスト実行
dotnet test

# カバレッジ付きで実行
dotnet test /p:CollectCoverage=true
```

**実行結果:**
- 合格: 42/42 ✓
- 失敗: 0
- 実行時間: ~35ms

## 今後の機能拡張

### データ永続化
- [ ] JSON形式でのエクスポート/インポート
- [ ] LocalStorageへの保存
- [ ] データベース連携（SQLite, PostgreSQL等）

### 複数トーナメント管理
- [ ] トーナメント一覧画面
- [ ] トーナメントの切り替え
- [ ] 過去のトーナメント履歴

### エクスポート機能
- [ ] 対戦表のPDF出力
- [ ] CSV形式での結果エクスポート
- [ ] 印刷用レイアウト

### 参加者管理機能
- [ ] 参加者の追加/削除/編集
- [ ] 参加者の途中棄権処理
- [ ] シード権の設定

### UI/UX改善
- [ ] リアルタイム更新（SignalR）
- [ ] モバイル対応の強化
- [ ] ダークモード対応
- [ ] 多言語対応（i18n）

### 統計機能
- [ ] 対戦履歴の詳細表示
- [ ] 参加者間の対戦成績マトリックス
- [ ] グラフやチャートでの可視化
- [ ] レーティングシステムの導入

### その他の拡張
- [ ] 複数トーナメント形式のサポート
  - [ ] シングルエリミネーション
  - [ ] ダブルエリミネーション
  - [ ] ラウンドロビン
- [ ] ユーザー認証・権限管理
- [ ] Web API化
- [ ] モバイルアプリ版

## アーキテクチャ設計

### 分離アーキテクチャ
- **UIレイヤー**: Blazor Components
- **ビジネスロジック**: TournamentEditor.Core（Blazor非依存）
- **テストレイヤー**: xUnit

### 利点
1. **Blazorからの独立**: Coreプロジェクトは純粋なC#ロジック
2. **テスタビリティ**: Web UIなしで高速なユニットテスト実行
3. **再利用性**: 他のUIフレームワーク（WPF、Console、Web API等）でも利用可能
4. **保守性**: ビジネスロジックとUI層が明確に分離

## ライセンス

このプロジェクトで使用しているライブラリ：
- **xUnit v3**: Apache 2.0 License
- **.NET**: MIT License
- **Bootstrap**: MIT License

## 開発ログ

詳細な開発履歴は [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md) を参照してください。

## 貢献

プルリクエストを歓迎します。大きな変更の場合は、まずissueを開いて変更内容を議論してください。

## 開発環境

- macOS (Darwin 24.6.0)
- .NET 10.0
- Blazor Server (Interactive Server Components)
- xUnit v3 (テストフレームワーク)

---

**開発日**: 2025-12-25
