# Project Learnings

<!--
記法ルール:
- 1項目1洞察。複数の学びを1行に詰めない
- 各項目の先頭に日付を必ず入れる（例: - 2026-08-24: ...）
- 上4セクションは「生の観察」の置き場。Consolidated Principles には
  統合パスで抽出した原則だけを置く。両者を混ぜない
-->

## Patterns That Work
（効いたやり方・型）

- 2026-08-31: Unityアセットの修正は [InitializeOnLoad] の一括Editorスクリプトより、.meta/.prefab のシリアライズ済みYAMLを直接編集する方が確実。ファイル内容を読んで適用済みかを即検証でき、実行タイミング（Play中・ドメインリロード順）に依存しない。
- 2026-08-31: FBXの埋め込みマテリアル名はバイナリを `([\x20-\x7e]+)\x00\x01Material` の正規表現でスキャンすれば取得でき、.meta の externalObjects でプロジェクト内マテリアルへ再マップできる。
- 2026-08-31: Unityエディタが別Spaceにある場合、`screencapture -l<windowID>`（Quartz CGWindowListでID取得）で前面化せずにウィンドウを撮影できる。ただしバックグラウンドのUnityは再描画しないため、映像は最後にフォーカスがあった時点のもの。

- 2026-08-31: Unityのレイキャスト系バグは、Playモードで再現しなくても .prefab のシリアライズ値（コライダー寸法・目線位置・スケール）を集めてレイ×カプセル距離を計算する小スクリプトで「旧実装は全ケースFalse／新実装は全ケースTrue」を机上で証明できる。修正前にバグ条件を数値で再現・否定でき、エディタ起動に依存しない。

- 2026-08-31: Unityを前面化してもログの反映には数十秒かかる。プロジェクト直下 Logs/Editor.log の行番号を控え、その行以降に対象アセットの Importing 行と「Mono: successfully reloaded assembly」が出て `error CS` が無いことまで確認して初めて「反映済み」と判断する。

- 2026-08-31: `tell application "Unity" to activate` は効かないことがある。`tell application "System Events" to set frontmost of (first process whose unix id is <pid>) to true` なら確実に前面化でき、Editor.log にアセットリフレッシュが流れる。

- 2026-08-31: Playモードの光量演出は、Unityを前面化した状態で `screencapture -l<windowID>` を短間隔で連写し、ゲームビュー領域の平均輝度を数値化すれば検証できる（雷フラッシュ＝ベースライン8→ピーク34の単発スパイクとして観測できた）。全フレームのmd5が一致したら「変化なし」ではなくUnityがバックグラウンドで再描画停止している判定不能状態。
- 2026-08-31: 新規アセット（.cs/.wav）は自前で .meta を書いてGUIDを固定すれば、Unityの取り込み前にシーンやプレハブのYAMLから参照を張れる。C#スクリプトのコンパイル反映は Assembly-CSharp.dll に `strings` で型名が入ったこと+ログの「Mono: successfully reloaded assembly」+`error CS` ゼロで確認できる。

## Mistakes to Avoid
（失敗と再発防止策）

- 2026-08-31: EditorApplication.delayCall の一括修正スクリプトは Play モード中に発火すると早期リターンして部分適用のまま残る（コントローラーは変更済み・プレハブは未保存、という中途半端な状態が実際に発生）。一時的な修正は完了確認後に即削除するか、最初からアセット直接編集にする。
- 2026-08-31: Unityが開いたままアセットファイルを外部編集すると SourceAssetDB の mtime 不整合で Import Error Code:(4) が出る。次のリフレッシュで自己解決するが、ログの後続リフレッシュ成功まで確認して初めて「反映済み」と判断できる。

## Domain Knowledge
（業務・仕様に関する事実）

- 2026-08-31: 赤ちゃんモデル（Crawling.fbx, mixamoリグ）の埋め込みマテリアル名は `5_meshes_Merge`。表示用マテリアルは Baby_URP.mat（URP Lit + Baby_Albedo/Baby_Normal）で、.meta の externalObjects で再マップ済み。
- 2026-08-31: ハイハイのような左右非対称ループでは、クリップの Root Rotation 基準が Body Orientation（keepOriginalOrientation:0）だと平均向きがヨーずれし、直進中もモデルが斜めを向く。Original（keepOriginalOrientation:1）にすると解消する。
- 2026-08-31: プレイヤーの見た目サイズは Player.prefab ルートの m_LocalScale で調整する（現在 2,2,2）。ルートを一様スケールすればコライダー・Humanoidアニメーションごと正しく拡大される。Main.unity のインスタンスはスケールをオーバーライドしていない。
- 2026-08-31: 敵の視認は Observer.cs（PointOfView プレハブ、Ghost目線0.75m/Gargoyle目線1.4m）のレイキャストのみで判定。狙い先はプレイヤーコライダーの bounds.center（赤ちゃんの寝そべりカプセルはワールドで y 0〜0.6m しかなく、旧実装の「ピボット+1m」狙いでは全距離で頭上を通過して絶対に当たらなかった）。

- 2026-08-31: プレイヤー移動の2バグの根本原因: (1) 移動中の滑り＝Crawling クリップの Root Transform Position (XZ) が Bake Into Pose（loopBlendPositionXZ:1）で、這い前進がポーズ側に焼き込まれ、ループごとに前進→スナップバックしてカプセルの等速移動とズレていた。(2) 入力なし移動＝dynamic Rigidbody（減衰0）に衝突でソルバーが与えた速度が残留し続けていた。対策はルートモーション駆動（loopBlendPositionXZ:0 + ApplyRootMotion:1 + OnAnimatorMove で deltaPosition.magnitude を使用）と、FixedUpdate での linearVelocity/angularVelocity ゼロ化。
- 2026-08-31: MovePosition で動かす dynamic Rigidbody は速度をリセットしない限り、衝突で得た velocity が入力ゼロでも永続する（MovePosition は velocity を上書きしない）。Kinematic 化は静的コライダーをすり抜けるため不可。毎 FixedUpdate の velocity ゼロ化が最小の対処。

- 2026-08-31: プレイヤーの移動入力は Player.prefab にシリアライズされた単体 InputAction（MoveAction）。キー割り当ての追加は .prefab の m_SingletonActionBindings に 2DVector コンポジット（m_Flags:4）+ 4方向パート（m_Flags:8）を追記するだけでよく、C# 変更不要。Main.unity 側にオーバーライドはない。
- 2026-08-31: プレイヤーの移動速度調整は Baby_Player_Controller の Crawling ステート m_Speed で行う（現在 1.5）。ルートモーション駆動のためアニメ再生速度と移動速度が常に一致し、足滑りなしで増減できる。

- 2026-08-31: ライティング演出の構成: 雷は Main.unity の Directional Light（通常 intensity 0、青白 0.78/0.83/1）に LightningFlash.cs + AudioSource(SFXThunder.wav) を載せ、RenderSettings.ambientLight(Flatモード、ほぼ黒 0.01/0.013/0.022)と連動フリッカー。幽霊は Ghost.prefab の子 Lantern_Light（ポイント、橙、range6/intensity3、影なし）、ガーゴイルは Gargoyle.prefab の子 Gaze_Light（スポット45°、赤、PointOfViewと同じ y1.4/下向き20°）。URPの AdditionalLightsPerObjectLimit は 8 に引き上げ済み。

## Open Questions
（未解決・要調査）

## Consolidated Principles
（統合パス専用。通常の更新処理から直接追記しない）
