# ArxCharacterShaders
build 4

# バリエーション
- AlphaCutout
- Fade
- FadeRefracted
- Opaque
- Outline
  - Opaque
  - AlphaCutout

Arktoonにあったその他バリエーションは調整中

# Arktoonからの主な変更点

## 追加
### Proximity Color Override
視点がメッシュに近くなった場合に、色を変色させることができます
※ここで指定した色は、空間のあらゆる光の影響を無視して反映されます。

## 変更・削除
- Shadow関連の計算を再構築
- Outlineを別シェーダーバリエーションに切り出した