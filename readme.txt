﻿2013.06.04 Ver5.9.0
(1) 終了時にオプション設定のテンポラリファイルが残ってしまうバグを修正($Remote.ini Tmp.ini)
(2) ThreadBaseTest追加
(3) プロキシPOP3及びプロキシSMTPの多重ログインによる誤動作を修正
(4) Webサーバにおいて、不正なリクエストのURLエンコードで発生する例外に対処
(5) Ftpサーバにおいて、LISTコマンドで発生する例外に対処
(6) プロキシーサーバにおけるメモリリークを修正

2013.06.13 Ver5.9.1
(1) WebサーバにおいSSIの#include指定で、CGI以外の入力でヘッダ処理をしてしまうバグを修正
(2) 旧バージョンのオプションの読み込みに失敗するバグを修正

2013.06.28 Ver5.9.2
(1) オプションの読み込み(プロキシSMTPの拡張設定)に失敗するバグを修正
(2) HTTPSサーバの動作不良を修正

2013.08.03 Ver5.9.3
(1)SMTPサーバにおいてヘッダ変換時に改行が削除されてしまうバグを修正
(2)SMTPサーバにおいてAUTHコマンドのパラメータが小文字に対応できていないバグを修正
(3)SMTPさーばにおいてメールボックスへの格納時のログを修正

2013.09.14 Ver5.9.4
(1)DNSサーバにおいて、CNAMEを返すサーバの再帰処理に失敗するバグを修正

2013.09.17 Ver5.9.5
(1)DNSサーバにおいて、再帰処理を理ファクタリング

2013.09.30 Ver5.9.6
(1)SMTPサーバにおいて、複数行にわたるヘッダの処理を修正
(2)WinAPIサーバ機能追加
(3)WebサーバにおいてCGI実行時に元の環境変数をすべて継承するように修正

2013.10.24 Ver5.9.7
(1)FTPサーバにおいて、２バイトコードのレスポンスをshift-jisでエンコードするように修正
(2)プロキシHTTPにおいて、大きなサイズのPOSTデータが欠落することがあるバグを修正


2013.11.20 Ver5.9.8
(1)メールボックスへの保存に失敗した際のログを修正
(2)WebApiServerにおいて、メールボックスのフォルダ指定が無い場合に例外が発生する問題を修正
(3)WebApiServerにおいて、メールサーバが起動していない時に発生する例外を修正
(4)WebApiServerのレスポンスを修正
(5)プロキシーHTTPにおいて、SSL接続時のURL制限を修正

2013.12.05 Ver5.9.9
(1)UIの言語設定(日本語か英語か)を、既定ではOS設定に従うオプション（デフォルト）を追加

2013.12.22 Ver6.0.0
(1)WebAPIオプション設定のタイプミス(pull request)
(2)オプション設定でデータ一覧にソート機能を追加
(3)SMTPサーバにメーリングリスト機能を追加

2013.12.26 Ver6.0.1
(1)メーリングリストにおいて、管理領域（フォルダ）の間違いを修正
(2)HTMLメールによるsubscribe及びconfirmの誤動作を修正

2014.01.11 Ver6.0.2
(1)ACLをFQDNで指定できるように仕様変更　
[例] *.exsample.com


2014.02.10 Ver6.0.3
(1) FTPサーバにおいて、RNTOコマンドで発生するディレクトリトラバーサルの問題を修正
(2) 「トンネルの追加と削除」オプション設定の「ACL」タブを削除（ACL設定は各トンネル設定ごと個別）

2014.02.22 Ver6.0.4
(1) FTPサーバにおいて、存在しないディレクトリ名を含む名前変更で例外が発生するバグを修正
(2) FTPサーバにおいて、RNFRが送られる前にRNTOが呼び出された場合に例外が発生するバグを修正
(3) FTPサーバにおいて、RNTOにおけるディレクトリトラバーサルの問題に対処

2014.02.24 Ver6.0.5
(1) Ver6.0.4におけるインストーラのバージョン確認バグを修正

2014.03.29 Ver6.0.6
(1) プロキシーTELNETにおいて、制御文字列の処理を修正（ＴｅｒａＴｅｒｍ対応）

2014.04.29 Ver6.0.7
(1)「オプション」ー「ログ表示」において「ログを生成しない」の設定が正常動作していないバグを修正(1)「オプション」ー「ログ表示」において「ログを生成しない」の設定が正常動作していないバグを修正

2014.09.20 Ver6.0.8
(1)HTTPプロキシーにおいて、gzip圧縮されたコンテンツで「コンテンツ制限」が誤動作するバグを修正

2014.09.22 Ver6.0.9
(1)メーリングリストにおいて、複数行にわたるSubjectの連番追加に失敗するバグを修正


2014.10.20 Ver6.1.0
(1)DNSサーバにおいて、ドメインを一部上書きする機能を追加

2015.01.30 Ver6.1.1
(1)zipタイプのパッケージで、サービス登録時にエラーが発生する問題を修正

2015.02.01 Ver6.1.2
(1)IPv4互換(射影)アドレスに対応

2015.02.19 Ver6.1.3
(1)メールサーバにおいて、ヘッダ行と本文の間に、空白行がないメールの処理に対応

2015.02.25 Ver6.1.4
(1)複数行にわたるReceivedヘッダの処理に失敗することがあるバグ(v6.1.3のみ)を修正

2015.03.01 Ver6,1,5
(1)複数行にわたるReceivedヘッダの処理に失敗することがあるバグ(v6.1.3 , v6.1.4のみ)を再修正

2015.03.19 Ver6.1.6
(1)言語ファイル（BJD.Lang.txt）を追加　一部の文字列を言語ファイルから読み込むように修正

2015.05.05 Ver6.1.7
(1)言語ファイル（BJD.Lang.txt）からの読み込みを追加

2015.07.28 Ver6.1.8
(1) サービス起動で異常終了するバグを修正
(2) 言語ファイルがない場合に例外が発生する問題を修正

2015.10.07 Ver6.1.9
(1) メールサーバのドメイン設定で、空白等が入っていても誤動作しないように修正
(2) VisualStudio2015において再構築
