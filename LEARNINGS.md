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

## Mistakes to Avoid
（失敗と再発防止策）

- 2026-08-31: EditorApplication.delayCall の一括修正スクリプトは Play モード中に発火すると早期リターンして部分適用のまま残る（コントローラーは変更済み・プレハブは未保存、という中途半端な状態が実際に発生）。一時的な修正は完了確認後に即削除するか、最初からアセット直接編集にする。
- 2026-08-31: Unityが開いたままアセットファイルを外部編集すると SourceAssetDB の mtime 不整合で Import Error Code:(4) が出る。次のリフレッシュで自己解決するが、ログの後続リフレッシュ成功まで確認して初めて「反映済み」と判断できる。

## Domain Knowledge
（業務・仕様に関する事実）

- 2026-08-31: 赤ちゃんモデル（Crawling.fbx, mixamoリグ）の埋め込みマテリアル名は `5_meshes_Merge`。表示用マテリアルは Baby_URP.mat（URP Lit + Baby_Albedo/Baby_Normal）で、.meta の externalObjects で再マップ済み。
- 2026-08-31: ハイハイのような左右非対称ループでは、クリップの Root Rotation 基準が Body Orientation（keepOriginalOrientation:0）だと平均向きがヨーずれし、直進中もモデルが斜めを向く。Original（keepOriginalOrientation:1）にすると解消する。
- 2026-08-31: プレイヤーの見た目サイズは Player.prefab ルートの m_LocalScale で調整する（現在 2,2,2）。ルートを一様スケールすればコライダー・Humanoidアニメーションごと正しく拡大される。Main.unity のインスタンスはスケールをオーバーライドしていない。

## Open Questions
（未解決・要調査）

## Consolidated Principles
（統合パス専用。通常の更新処理から直接追記しない）
