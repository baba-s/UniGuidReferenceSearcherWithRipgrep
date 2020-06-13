# UniGuidReferenceSearcherWithRipgrep

ripgrep を使用して指定したアセットを参照している他のアセットを検索するエディタ拡張  

## 使用例

```cs
using Kogane;
using UnityEditor;
using UnityEngine;

public static class Example
{
	[MenuItem( "Tools/Hoge" )]
	private static void Hoge()
	{
		var searcher = new GuidReferenceSearcherWithRipgrep
		(
			rgExePath: "rg.exe", // rg.exe のファイルパス
			ignoreFilePath: "ignore.txt" // 無視ファイルのパス（指定しない場合は空文字列を渡す）
		);

		var references = searcher.Search
		(
			assetPath: "Assets/Square.png", // 依存関係を検索したいアセットのパス
			searchFolderPath: "Assets" // 依存関係を検索する対象のフォルダ
		);

		foreach ( var reference in references )
		{
			Debug.Log( reference );
		}
	}
}
```

## 無視ファイル

```txt
*.meta
```

* 例えば .meta は検索から除外したい場合は無視ファイルに上記のように記述します    

## 謝辞

* このリポジトリは ripgrep を使用させていただいています  
    * https://github.com/BurntSushi/ripgrep
