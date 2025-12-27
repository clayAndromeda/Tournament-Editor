# 参加者管理機能ドキュメント

## 概要

トーナメント参加者を管理するためのデータベースシステムと抽選機能を実装しました。Entity Framework Core + SQLiteを使用し、将来の拡張性を考慮したモジュール設計となっています。

## アーキテクチャ

### 階層構造

```
TournamentEditor.Core/
├── Data/
│   ├── Entities/
│   │   └── ParticipantEntity.cs          # 参加者エンティティ
│   ├── Repositories/
│   │   ├── IRepository.cs                # 汎用リポジトリインターフェース
│   │   ├── Repository.cs                 # 汎用リポジトリ実装
│   │   ├── IParticipantRepository.cs     # 参加者リポジトリインターフェース
│   │   └── ParticipantRepository.cs      # 参加者リポジトリ実装
│   └── TournamentDbContext.cs            # データベースコンテキスト
└── Services/
    ├── ParticipantManagementService.cs   # 参加者管理サービス
    └── LotteryService.cs                 # 抽選サービス
```

### レイヤー設計

1. **Entity Layer**: データベーステーブルの定義
2. **Repository Layer**: データアクセスの抽象化（CRUD操作）
3. **Service Layer**: ビジネスロジック（参加者管理、抽選）

## 主要機能

### 1. 参加者エンティティ（ParticipantEntity）

データベースに保存される参加者情報。

#### プロパティ

- `Id` (Guid): 参加者ID（主キー）
- `Name` (string): 参加者名（必須、最大100文字）
- `RegisteredAt` (DateTime): 登録日時
- `Email` (string?): メールアドレス（オプション、最大200文字）
- `PhoneNumber` (string?): 電話番号（オプション、最大20文字）
- `Notes` (string?): メモ（オプション、最大1000文字）
- `IsActive` (bool): アクティブ状態（デフォルト: true）
- `CreatedAt` (DateTime): 作成日時
- `UpdatedAt` (DateTime): 更新日時

### 2. リポジトリパターン

#### 汎用リポジトリインターフェース（IRepository<T>）

```csharp
Task<T?> GetByIdAsync(Guid id);
Task<IEnumerable<T>> GetAllAsync();
Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
Task<T> AddAsync(T entity);
Task AddRangeAsync(IEnumerable<T> entities);
Task UpdateAsync(T entity);
Task DeleteAsync(T entity);
Task DeleteByIdAsync(Guid id);
Task<int> CountAsync();
Task<int> CountAsync(Expression<Func<T, bool>> predicate);
Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
```

#### 参加者リポジトリ（IParticipantRepository）

```csharp
Task<IEnumerable<ParticipantEntity>> GetActiveParticipantsAsync();
Task<IEnumerable<ParticipantEntity>> SearchByNameAsync(string name);
Task<IEnumerable<ParticipantEntity>> GetRecentParticipantsAsync(int count = 10);
Task<IEnumerable<ParticipantEntity>> DrawRandomParticipantsAsync(int count, bool activeOnly = true);
```

### 3. 参加者管理サービス（ParticipantManagementService）

参加者の登録、更新、削除、検索機能を提供。

#### 主要メソッド

```csharp
// 参加者を登録
Task<ParticipantEntity> RegisterParticipantAsync(
    string name,
    string? email = null,
    string? phoneNumber = null,
    string? notes = null);

// 複数の参加者を一括登録
Task RegisterParticipantsAsync(IEnumerable<string> names);

// 参加者情報を更新
Task<ParticipantEntity?> UpdateParticipantAsync(
    Guid id,
    string? name = null,
    string? email = null,
    string? phoneNumber = null,
    string? notes = null,
    bool? isActive = null);

// 参加者を削除
Task DeleteParticipantAsync(Guid id);

// すべての参加者を取得
Task<IEnumerable<ParticipantEntity>> GetAllParticipantsAsync();

// アクティブな参加者を取得
Task<IEnumerable<ParticipantEntity>> GetActiveParticipantsAsync();

// 名前で検索
Task<IEnumerable<ParticipantEntity>> SearchParticipantsByNameAsync(string name);

// 参加者数を取得
Task<int> GetParticipantCountAsync(bool activeOnly = false);
```

### 4. 抽選サービス（LotteryService）

参加者のランダム抽選機能を提供。

#### 抽選結果（LotteryResult）

```csharp
public class LotteryResult
{
    public List<ParticipantEntity> SelectedParticipants { get; set; }
    public DateTime DrawnAt { get; set; }
    public int RequestedCount { get; set; }
    public int TotalParticipants { get; set; }
    public bool ActiveOnly { get; set; }
}
```

#### 主要メソッド

```csharp
// 基本的な抽選
Task<LotteryResult> DrawLotteryAsync(int count, bool activeOnly = true);

// 条件付き抽選（カスタムフィルタ）
Task<LotteryResult> DrawLotteryWithFilterAsync(
    int count,
    Func<ParticipantEntity, bool> filter);

// 複数回抽選（重複なし）
Task<List<LotteryResult>> DrawMultipleRoundsAsync(
    int rounds,
    int countPerRound,
    bool activeOnly = true);
```

## 使用例

### 基本的な使い方

```csharp
// DIコンテナからサービスを取得
@inject ParticipantManagementService ParticipantService
@inject LotteryService LotteryService

// 参加者を登録
var participant = await ParticipantService.RegisterParticipantAsync(
    name: "山田太郎",
    email: "yamada@example.com",
    phoneNumber: "090-1234-5678"
);

// 複数の参加者を一括登録
var names = new[] { "佐藤花子", "鈴木次郎", "田中三郎" };
await ParticipantService.RegisterParticipantsAsync(names);

// アクティブな参加者を取得
var activeParticipants = await ParticipantService.GetActiveParticipantsAsync();

// 参加者を検索
var searchResults = await ParticipantService.SearchParticipantsByNameAsync("山田");

// 参加者情報を更新
await ParticipantService.UpdateParticipantAsync(
    id: participant.Id,
    email: "new-email@example.com"
);
```

### 抽選機能の使い方

```csharp
// 基本的な抽選（アクティブな参加者から3人を抽選）
var result = await LotteryService.DrawLotteryAsync(count: 3, activeOnly: true);

Console.WriteLine($"抽選結果: {result.SelectedParticipants.Count}人選出");
Console.WriteLine($"総参加者数: {result.TotalParticipants}人");
foreach (var participant in result.SelectedParticipants)
{
    Console.WriteLine($"- {participant.Name}");
}

// 条件付き抽選（メールアドレスが登録されている参加者から抽選）
var emailResult = await LotteryService.DrawLotteryWithFilterAsync(
    count: 5,
    filter: p => !string.IsNullOrEmpty(p.Email)
);

// 複数回抽選（3回に分けて各2人ずつ、重複なし）
var multiRoundResults = await LotteryService.DrawMultipleRoundsAsync(
    rounds: 3,
    countPerRound: 2,
    activeOnly: true
);

for (int i = 0; i < multiRoundResults.Count; i++)
{
    Console.WriteLine($"\n第{i + 1}回抽選:");
    foreach (var participant in multiRoundResults[i].SelectedParticipants)
    {
        Console.WriteLine($"- {participant.Name}");
    }
}
```

## データベース設定

### 接続文字列

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tournament.db"
  }
}
```

### 初期化

アプリケーション起動時に自動的にデータベースが作成されます（[Program.cs:31-35](TournamentEditor/Program.cs#L31-L35)）。

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TournamentDbContext>();
    dbContext.Database.EnsureCreated();
}
```

### マイグレーション（将来の拡張用）

現在は `EnsureCreated()` を使用していますが、本番環境ではマイグレーションの使用を推奨します。

```bash
# マイグレーションの作成
dotnet ef migrations add InitialCreate --project TournamentEditor.Core

# データベースの更新
dotnet ef database update --project TournamentEditor
```

## DIコンテナの設定

[Program.cs:14-26](TournamentEditor/Program.cs#L14-L26) でサービスが登録されています。

```csharp
// Database
builder.Services.AddDbContext<TournamentDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=tournament.db"));

// Repositories
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();

// Services
builder.Services.AddScoped<ParticipantManagementService>();
builder.Services.AddScoped<LotteryService>();
```

## 拡張性

### 新しいエンティティの追加

1. `Data/Entities/` に新しいエンティティクラスを作成
2. `TournamentDbContext` に `DbSet<T>` を追加
3. `OnModelCreating` で設定を追加
4. 必要に応じて専用のRepositoryを作成

### 新しいサービスの追加

1. `Services/` に新しいサービスクラスを作成
2. コンストラクタでRepositoryを注入
3. `Program.cs` でDIに登録

### カスタムクエリの追加

`IParticipantRepository` にメソッドを追加し、`ParticipantRepository` で実装:

```csharp
// インターフェースに追加
Task<IEnumerable<ParticipantEntity>> GetParticipantsRegisteredAfterAsync(DateTime date);

// 実装
public async Task<IEnumerable<ParticipantEntity>> GetParticipantsRegisteredAfterAsync(
    DateTime date,
    CancellationToken cancellationToken = default)
{
    return await _dbSet
        .Where(p => p.RegisteredAt > date)
        .OrderBy(p => p.RegisteredAt)
        .ToListAsync(cancellationToken);
}
```

## セキュリティとベストプラクティス

1. **入力検証**: サービス層で参加者名などの入力を検証
2. **非同期処理**: すべてのDB操作は非同期メソッドを使用
3. **CancellationToken**: 長時間実行される操作でキャンセルをサポート
4. **トランザクション**: 複雑な操作では `DbContext.Database.BeginTransaction()` を使用
5. **インデックス**: 頻繁に検索されるカラム（Name, IsActive, RegisteredAt）にインデックスを設定済み

## トラブルシューティング

### データベースファイルが見つからない

アプリケーションの実行ディレクトリに `tournament.db` が作成されます。パスを確認してください。

### マイグレーションエラー

`EnsureCreated()` とマイグレーションは併用できません。本番環境ではマイグレーションのみを使用してください。

### パフォーマンス問題

大量の参加者がいる場合、ページネーションや遅延読み込みの実装を検討してください。

## 今後の拡張案

- [ ] 参加者のインポート/エクスポート機能（CSV, Excel）
- [ ] 参加者グループ管理
- [ ] 抽選履歴の保存と再現
- [ ] 参加者の統計情報（参加回数、勝率など）
- [ ] メール通知機能
- [ ] 参加者のQRコード生成
- [ ] 参加者の写真管理
