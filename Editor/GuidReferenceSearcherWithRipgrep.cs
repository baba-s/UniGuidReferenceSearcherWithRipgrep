using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;

namespace Kogane
{
	/// <summary>
	/// 指定されたアセットを参照している他のアセットを検索するクラス
	/// ripgrep を使用して GUID ベースで参照を検索します
	/// </summary>
	public sealed class GuidReferenceSearcherWithRipgrep
	{
		//==============================================================================
		// 変数(readonly)
		//==============================================================================
		private readonly string m_rgExePath;      // rg.exe のファイルパス
		private readonly string m_ignoreFilePath; // 無視ファイルのパス

		//==============================================================================
		// 関数
		//==============================================================================
		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="rgExePath">rg.exe のファイルパス</param>
		public GuidReferenceSearcherWithRipgrep( string rgExePath )
			: this( rgExePath, string.Empty )
		{
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="rgExePath">rg.exe のファイルパス</param>
		/// <param name="ignoreFilePath">無視ファイルのパス</param>
		public GuidReferenceSearcherWithRipgrep( string rgExePath, string ignoreFilePath )
		{
			m_rgExePath      = rgExePath;
			m_ignoreFilePath = ignoreFilePath ?? string.Empty;
		}

		/// <summary>
		/// 指定されたアセットを参照している他のアセットを検索してリストで返します
		/// ripgrep を使用して GUID ベースで参照を検索します
		/// </summary>
		/// <param name="assetPath">対象のアセットのパス</param>
		/// <param name="searchFolderPath">参照を検索するフォルダのパス</param>
		/// <returns>指定されたアセットを参照している他のアセットのパスのリスト</returns>
		public string[] Search
		(
			string assetPath,
			string searchFolderPath
		)
		{
			var threads   = Environment.ProcessorCount;
			var assetGuid = AssetDatabase.AssetPathToGUID( assetPath );

			var arguments = $@"--case-sensitive --files-with-matches --fixed-strings --follow --no-text --ignore-file {m_ignoreFilePath} --threads {threads} --regexp {assetGuid} -- {searchFolderPath}";

			var processStartInfo = new ProcessStartInfo
			{
				FileName               = m_rgExePath,
				CreateNoWindow         = true,
				Arguments              = arguments,
				UseShellExecute        = false,
				RedirectStandardOutput = true,
			};

			var references    = new List<string>();
			var process       = new Process { StartInfo = processStartInfo };
			var assetMetaPath = ( assetPath + ".meta" ).Replace( "/", "\\" );

			// ripgrep は検索対象の文字列が含まれているファイルを見つけたらコンソールにパスを出力します
			// そのため OutputDataReceived を使用することで、
			// 指定されたアセットを参照している他のアセットのパスを検知できます
			void OnOutputDataReceived( object sender, DataReceivedEventArgs e )
			{
				var path = e.Data;

				if ( string.IsNullOrWhiteSpace( path ) ) return;
				if ( path == assetMetaPath ) return;

				references.Add( path.Replace( "\\", "/" ) );
			}

			process.OutputDataReceived += OnOutputDataReceived;

			// ripgrep を実行します
			process.Start();
			process.BeginOutputReadLine();

			// ripgrep が完了するまで待ちます
			while ( !process.HasExited )
			{
			}

			return references.ToArray();
		}
	}
}