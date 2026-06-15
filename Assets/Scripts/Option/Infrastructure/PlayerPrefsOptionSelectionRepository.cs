// ======================================================
// PlayerPrefsOptionSelectionRepository.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-13
// 更新日時 : 2026-05-13
// 概要     : PlayerPrefs による選択インデックス保存
// ======================================================

using UnityEngine;
using OptionSystem.Domain;

namespace OptionSystem.Infrastructure
{
    /// <summary>
    /// PlayerPrefs による選択インデックス保存
    /// </summary>
    public sealed class PlayerPrefsOptionSelectionRepository : IOptionSelectionRepository, IOptionSelectionIndexReader
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>セーブデータ存在フラグキー</summary>
        private const string KEY_EXISTS = "GAME_OPTION_EXISTS";

        /// <summary>PlayerPrefs キー接頭辞</summary>
        private const string KEY_PREFIX = "OPTION_SELECTION_";

        /// <summary>デフォルトインデックス</summary>
        private const int DEFAULT_INDEX = -1;
        
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// セーブデータが存在するか判定
        /// </summary>
        public bool HasSavedData()
        {
            // セーブ済みフラグを参照する
            // 未設定時は 0
            return PlayerPrefs.GetInt(KEY_EXISTS, 0) == 1;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 選択インデックスを保存する
        /// </summary>
        /// <param name="type">オプション種別</param>
        /// <param name="index">保存するインデックス</param>
        public void Save(in OptionType type, int index)
        {
            string key = CreateKey(type);

            PlayerPrefs.SetInt(key, index);

            PlayerPrefs.Save();
        }

        /// <summary>
        /// 選択インデックス取得
        /// </summary>
        public int Load(in OptionType type)
        {
            string key = CreateKey(type);

            return PlayerPrefs.GetInt(key, DEFAULT_INDEX);
        }

        /// <summary>
        /// 選択インデックス取得
        /// IOptionSelectionIndexReader 用
        /// </summary>
        public int Get(in OptionType type)
        {
            return Load(type);
        }

        /// <summary>
        /// 保存データが存在するか判定する
        /// </summary>
        /// <param name="type">オプション種別</param>
        /// <returns>存在する場合 true</returns>
        public bool Exists(in OptionType type)
        {
            string key = CreateKey(type);

            // キーが存在しない場合は未設定
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }

            int value = PlayerPrefs.GetInt(key, -1);

            // 初期値は未設定扱い
            if (value == DEFAULT_INDEX)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// PlayerPrefs をリセットする
        /// </summary>
        public void Delete()
        {
            foreach (OptionType type in System.Enum.GetValues(typeof(OptionType)))
            {
                // 各オプションの保存データを削除
                PlayerPrefs.DeleteKey(CreateKey(type));
            }

            // セーブ存在フラグも削除
            PlayerPrefs.DeleteKey(KEY_EXISTS);

            PlayerPrefs.Save();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// オプション種別用キーを生成する
        /// </summary>
        /// <param name="type">オプション種別</param>
        /// <returns>PlayerPrefs キー</returns>
        private string CreateKey(in OptionType type)
        {
            // enum 名をそのままキー化
            return $"{KEY_PREFIX}{type}";
        }
    }
}