# VRCPhotoAlbum


<img src="https://github.com/gatosyocora/VRCPhotoAlbum/blob/develop/images/VRCPhotoAlbum_previewimage.png" width="50%"/>


VRChatで撮影した写真をながめるためのWindows向けアプリケーションです

[vrc_meta_tool](https://github.com/27Cobalter/vrc_meta_tool)でメタ情報をいれた写真であれば,  
撮影したユーザーやワールド, 日時の情報を表示したり, それらを元に検索したりできます.

latest release : ver 0.3

## 画面構成

### 写真一覧画面
<img src="https://github.com/gatosyocora/VRCPhotoAlbum/blob/develop/images/VRCPhotoAlbum_topimage.png" width="40%"/>

### 写真詳細画面
<img src="https://github.com/gatosyocora/VRCPhotoAlbum/blob/develop/images/VRCPhotoAlbum_previewimage.png" width="40%"/>

### 設定画面
<img src="https://github.com/gatosyocora/VRCPhotoAlbum/blob/develop/images/VRCPhotoAlbum_settingimage.png" width="40%"/>

## 使い方
### 初回起動時
1. 設定画面が表示されるのでフォルダパス横の「参照」を選択し, 写真が入ったフォルダを選択します.
2. 「適用」を選択します.


### 写真を表示
1. 写真一覧画面で写真を選択する.
2. 写真詳細画面が表示される.


### 写真を検索
A. 写真一覧画面上部の検索欄に検索したいユーザー名またはワールド名を入力する.  
(入力した文字が部分的に一致するユーザーとワールドを検索します)

B. 写真詳細画面のユーザー名またはワールド名横のボタン, 日時横のボタンを選択する.  
(選択したものに完全一致するものを検索します)

C. 写真一覧画面にある日付入力欄で日付を選択する.  
または「今日」ボタンを選択する.  
(選択した日付に撮影されたものを検索します)

D. 写真一覧画面にある「今週」「今月」ボタンを選択する.  
(選択した期間に撮影されたものを検索します)

### 

## 更新履歴
(ver0.3)
* アプリケーションを一新(従来の機能はなくなった)
* vrc_meta_toolで埋め込んだ情報を表示できるように
* ユーザー, ワールド, 日付, 期間で検索できるように

(ver0.2)
* 常駐アプリ化(インジケーターに表示されるようになった, 右クリックでウィンドウ表示,写真整理,終了が可能)
* スタートアップに登録/解除(インジケーターのアイコン右クリックからおこなう)

(ver0.1)
* 写真を日付ごとのフォルダに分ける
* 日付ごとのフォルダの一覧を表示（それぞれに含まれる写真の数を表示）
* 写真の枚数順にフォルダ一覧を並べ替え
* 日付順にフォルダ一覧を並べ替え
* 特定のフォルダの写真一覧をサムネイル付きで表示
* 写真を選択すると既存のアプリケーションで開く

## 利用規約など
MITライセンスです。詳しくは[LICENSE](https://github.com/gatosyocora/VRCPhotoAlbum/blob/master/LICENSE)へ

## インストール
1. [releases](https://github.com/gatosyocora/VRCPhotoAlbum/releases)からVRCPhotoAlbum_vXXX.zipをダウンロードして解凍(XXXはバージョン)
2. VRCPhotoAlbum.exeを起動する

## アンインストール
インストールしたフォルダごと削除する

## 利用ライブラリ
* VrcMetaTool.NET
* ReactiveProperty
* Microsoft.Xaml.Behaviors.Wpf
* MaterialDesignThemes.MahApps
* MaterialDesignThemes
* MaterialDesignColors
* MahApps.Metro
