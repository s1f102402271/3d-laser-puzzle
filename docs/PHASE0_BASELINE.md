# フェーズ0 基準記録

> 記録日: 2026-08-18  
> 基準コミット: `7e6b391`  
> Unity: `6000.4.5f1`  
> 対象シーン: `Assets/_Project/Scenes/Prototype/Prototype_01.unity`

この文書は、フェーズ1以降の変更で完成済みのプロトタイプ機能が壊れていないか確認するための基準である。ゲーム仕様は `GAME_SPEC.md` を正とし、この文書では現在の実装と確認手順だけを扱う。

作成時点でC#コンパイル（Warning 0、Error 0）と基準経路の配置計算は確認済みである。以下のPlayモード用チェックボックスは、最初の手動回帰確認時に記録するため未チェックのまま残す。

## 1. 現在のプロトタイプ

### 操作

| 操作 | 入力 | 現在の挙動 |
|---|---|---|
| 移動 | `W/A/S/D` | 一人称視点で歩行する |
| 視点操作 | マウス | 一人称カメラを上下左右へ動かす |
| 一人称視点 | `1` | FirstPersonCameraへ切り替える |
| 俯瞰視点 | `2` | TopDownCameraへ切り替え、プレイヤー移動を停止する |
| 俯瞰カメラ移動 | `W/A/S/D` または矢印キー | X/Z平面上を固定範囲内で平行移動する |
| 発射装置の配置・再配置 | 画面中央でエリアを狙い、左クリック | 1台を生成し、以後のクリックでは同じ装置を移動する |

ジャンプ、しゃがみ、俯瞰ズーム、発射角度操作、発射後の位置固定は未実装であり、フェーズ0の回帰対象には含めない。

### シーン構成

- 配置候補: `PlacementArea_01`、`PlacementArea_02`、`PlacementArea_03` の3地点
- 発射装置: `Assets/_Project/Prefabs/LaserLauncher.prefab`
- 反射面: `ReflectiveWall`。通常のColliderとしてレーザーを反射する
- 遮断面: `BlockingWall`。`LaserBlockingWall`によって反射せず停止する
- ゴール: `Goal`。照射中は`ClearText`を表示する
- レーザー: 常時照射、最大距離20m、最大反射3回

### 基準クリア手順

1. `Prototype_01` を開いてPlayモードを開始する。
2. `2`で俯瞰視点に切り替え、3つの青い配置地点と壁、ゴールの位置を確認する。
3. `1`で一人称視点へ戻る。
4. 中央の`PlacementArea_02`を画面中央で狙い、左クリックする。
5. 生成されたレーザーが`ReflectiveWall`で1回反射し、`Goal`へ到達することを確認する。
6. 画面に`CLEAR`が表示されることを確認する。

基準化前は`ReflectiveWall`がY=25度で、中央配置のレーザーが反射後に`BlockingWall`へ到達していた。ゴール位置が想定する経路と整合するY=45度へ補正し、上記を基準経路とした。

## 2. 回帰確認チェックリスト

### 起動・Console

- [ ] `Prototype_01`を開いた状態でスクリプトコンパイルが完了する
- [ ] Playモード開始時にプロジェクトコード由来のErrorがない
- [ ] Playモード終了まで、プロジェクトコード由来のErrorが増えない
- [ ] `LaserLauncherPlacer`の参照不足Errorが出ない

### 移動

- [ ] 一人称視点で`W/A/S/D`の方向へ歩ける
- [ ] 斜め移動が単軸移動より速くならない
- [ ] マウスで左右を向ける
- [ ] マウスで上下を向け、真上・真下を越えない

### カメラ

- [ ] 開始時は一人称視点である
- [ ] `2`で真上固定の俯瞰視点へ切り替わる
- [ ] 俯瞰視点中はプレイヤーが移動しない
- [ ] 俯瞰視点中はカメラだけを平行移動できる
- [ ] 俯瞰カメラが設定範囲外へ移動せず、傾かない
- [ ] `1`で一人称視点へ戻り、移動と視点操作が再開する

### 配置

- [ ] 画面中央で配置エリアを狙ったときだけ配置候補を取得する
- [ ] 左クリックで選択中の配置地点へ1台だけ生成する
- [ ] 別の配置地点で左クリックしても2台目を生成せず、既存の1台を移動する
- [ ] 配置地点以外を狙った左クリックでは装置を生成・移動しない

### レーザー・反射・遮断・ゴール

- [ ] レーザーが発射地点から常時表示される
- [ ] `PlacementArea_02`から反射壁へ到達する
- [ ] 反射面の法線に応じた方向へ1回反射する
- [ ] 反射後にゴールへ到達し、`CLEAR`が表示される
- [ ] ゴールからレーザーを外すと`CLEAR`が非表示になる
- [ ] `LaserBlockingWall`を持つ面では反射せず停止する
- [ ] 最大反射回数を超えて処理せず、Editorが停止しない

## 3. Console基準

2026-08-18に、Unity Editor起動からシーン読込完了後までの`Editor.log`を確認した。

### プロジェクトコード

- C#コンパイルError: 0件
- C#コンパイルWarning: 0件
- `NullReferenceException` / `MissingReferenceException`: 0件
- `Debug.LogError`実行: 0件

### Unity・パッケージ由来の既存ログ

| ログ | 回数 | 扱い |
|---|---:|---|
| Licensing Clientの初回handshake失敗 | 1組 | 直後に再接続・ライセンス取得成功。プロジェクト回帰には数えない |
| `Attempted to call .Dispose on an already disposed CancellationTokenSource` | 3 | Android Extensionの端末スキャン付近。プロジェクトコード由来ではない |
| Test用assemblyの`not valid. Loading of assembly skipped.` | 29 | Packageのテストassembly読込時。通常の`Assembly-CSharp`コンパイルは成功 |
| D3D12 info queue取得失敗 | 1 | Editor起動時の環境ログ。描画初期化は継続している |

今後の変更では「既存ログだから無視する」のではなく、同じ発生元・同じ内容であることを確認する。プロジェクトコード由来の新しいErrorは0件を維持する。

## 4. Inspectorで調整できる基準値

以下はすべて`SerializeField`であり、コードを変更せずInspectorから調整できる。

| コンポーネント | 項目 | 現在値 | 位置づけ |
|---|---|---:|---|
| `FirstPersonController` | Move Speed | 4 | 歩行速度 |
| `FirstPersonController` | Look Sensitivity | 0.1 | 仮の操作感 |
| `FirstPersonController` | Max Look Angle | 80 | 上下視点制限 |
| `TopDownCameraController` | Pan Speed | 8 | 仮の操作感 |
| `TopDownCameraController` | Minimum / Maximum Position | (-8,-8) / (8,8) | カメラ移動範囲 |
| `TopDownCameraController` | Fixed Orthographic Size | 10 | ズーム実装前の仮値 |
| `LaserLauncherPlacementAimer` | Max Aim Distance | 10 | 配置照準距離 |
| `LaserLauncherPlacementAimer` | Placement Area Mask | 51 | 配置照準の対象Layer |
| `LaserLauncherPlacementArea` | Area Size | (1,1) | Prefab基準サイズ |
| `LaserLauncherPlacementArea` | Collider Thickness | 1 | Prefab基準の判定厚さ |
| `LaserLauncherPlacementArea` | Point Height Offset | 0.3 | 配置点の高さ補正 |
| `StraightLaser` | Max Distance | 20 | 未確定の最大距離 |
| `StraightLaser` | Collision Mask | Everything | 命中対象Layer |
| `StraightLaser` | Trigger Interaction | Ignore | Triggerを無視 |
| `StraightLaser` | Max Reflections | 3 | 仮の安全上限 |
| `StraightLaser` | Reflection Offset | 0.001 | 同一面への再命中防止 |
| `StraightLaser` | Width | 0.05 | 仮の表示幅 |
| `StraightLaser` | Color | Red | 仮の表示色 |

`LaserLauncher.prefab`の値を変えると、実行中に新しく配置する装置へ反映される。シーン内で無効化されている旧`LaserEmitter`は基準経路に使用しない。

## 5. 既知の制約

- Build Settingsには`SampleScene`だけが登録されており、`Prototype_01`は未登録。Editorで対象シーンを開いてPlayする基準とする。
- 発射角度をプレイヤーが変更する機能は未実装。現フェーズでは配置地点と固定角度で基準経路を成立させる。
- `CLEAR`は照射中の表示切り替えだけであり、ステージ進行は未実装。
- ジャンプ、しゃがみ、移動可能範囲、俯瞰ズームはフェーズ4の対象。
- 自動回帰テストはまだない。フェーズ0ではこの手動チェックリストを基準とし、経路データ分離後に自動化を検討する。

## 6. 実施記録テンプレート

```markdown
### フェーズ0回帰確認 YYYY-MM-DD

- Unity:
- 対象コミット:
- 対象シーン: Assets/_Project/Scenes/Prototype/Prototype_01.unity
- 基準クリア: OK / NG
- 移動: OK / NG
- カメラ: OK / NG
- 配置・再配置: OK / NG
- 反射: OK / NG
- 遮断: OK / NG
- ゴール表示: OK / NG
- 新規Error: 0 / 件数と内容
- 新規Warning: 0 / 件数と内容
- 備考:
```
