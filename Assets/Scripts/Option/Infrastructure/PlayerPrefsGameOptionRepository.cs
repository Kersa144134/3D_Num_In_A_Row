// ======================================================
// PlayerPrefsGameOptionRepository.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-09-24
// 更新日時 : 2026-04-27
// 概要     : PlayerPrefs によるゲーム設定実装
// ======================================================

using UnityEngine;
using OptionSystem.Domain;

namespace OptionSystem.Infrastructure
{
    /// <summary>
    /// PlayerPrefs を用いたゲーム設定の永続化実装
    /// </summary>
    public class PlayerPrefsGameOptionRepository : IGameOptionRepository
    {
        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // キー
        // --------------------------------------------------
        /// <summary>セーブデータ存在フラグキー</summary>
        private const string KEY_EXISTS = "GAME_OPTION_EXISTS";

        /// <summary>プレイヤー人数の保存キー</summary>
        private const string KEY_PLAYER_COUNT = "GAME_OPTION_PLAYER_COUNT";

        /// <summary>ターン数の保存キー</summary>
        private const string KEY_TURN_COUNT = "GAME_OPTION_TURN_COUNT";

        /// <summary>制限時間の保存キー</summary>
        private const string KEY_LIMIT_TIME = "GAME_OPTION_LIMIT_TIME";

        /// <summary>盤面サイズの保存キー</summary>
        private const string KEY_BOARD_SIZE = "GAME_OPTION_BOARD_SIZE";

        /// <summary>ライン成立条件の保存キー</summary>
        private const string KEY_CONNECT_COUNT = "GAME_OPTION_CONNECT_COUNT";

        /// <summary>カメラ速度の保存キー</summary>
        private const string KEY_CAMERA_SPEED = "GAME_OPTION_CAMERA_SPEED";

        /// <summary>ポインター速度の保存キー</summary>
        private const string KEY_POINTER_SPEED = "GAME_OPTION_POINTER_SPEED";

        // --------------------------------------------------
        // 初期値
        // --------------------------------------------------
        /// <summary>プレイヤー人数 デフォルト値</summary>
        private const int DEFAULT_PLAYER_COUNT = 2;

        /// <summary>ターン数 デフォルト値</summary>
        private const int DEFAULT_TURN_COUNT = 20;

        /// <summary>制限時間 デフォルト値</summary>
        private const float DEFAULT_LIMIT_TIME = 30f;

        /// <summary>盤面サイズ デフォルト値</summary>
        private const GameRules.BoardSizeType DEFAULT_BOARD_SIZE = GameRules.BoardSizeType.Size5;

        /// <summary>ライン成立条件 デフォルト値</summary>
        private const int DEFAULT_CONNECT_COUNT = 3;

        /// <summary>カメラ回転速度 デフォルト値</summary>
        private const float DEFAULT_CAMERA_ROTATION_SPEED = 360f;

        /// <summary>ポインター速度 デフォルト値</summary>
        private const float DEFAULT_POINTER_SPEED = 1000f;

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
        /// ゲームルールを PlayerPrefs へ保存する
        /// </summary>
        public void Save(GameRules rules)
        {
            PlayerPrefs.SetInt(KEY_PLAYER_COUNT, rules.PlayerCount);
            PlayerPrefs.SetInt(KEY_TURN_COUNT, rules.TurnCount);
            PlayerPrefs.SetFloat(KEY_LIMIT_TIME, rules.PerPlayerLimitTime);
            PlayerPrefs.SetInt(KEY_BOARD_SIZE, (int)rules.BoardSize);
            PlayerPrefs.SetInt(KEY_CONNECT_COUNT, rules.ConnectCount);
            PlayerPrefs.SetFloat(KEY_CAMERA_SPEED, rules.CameraSpeed);
            PlayerPrefs.SetFloat(KEY_POINTER_SPEED, rules.PointerSpeed);

            // セーブ済みフラグを立てる
            PlayerPrefs.SetInt(KEY_EXISTS, 1);

            // 書き込み
            PlayerPrefs.Save();
        }

        /// <summary>
        /// PlayerPrefs からゲームルールを復元する
        /// </summary>
        public GameRules Load()
        {
            GameRules rules = new GameRules();

            // 保存値を取得
            rules.PlayerCount = PlayerPrefs.GetInt(KEY_PLAYER_COUNT, DEFAULT_PLAYER_COUNT);
            rules.TurnCount = PlayerPrefs.GetInt(KEY_TURN_COUNT, DEFAULT_TURN_COUNT);
            rules.PerPlayerLimitTime = PlayerPrefs.GetFloat(KEY_LIMIT_TIME, DEFAULT_LIMIT_TIME);
            rules.BoardSize = (GameRules.BoardSizeType)PlayerPrefs.GetInt(KEY_BOARD_SIZE, (int)DEFAULT_BOARD_SIZE);
            rules.ConnectCount = PlayerPrefs.GetInt(KEY_CONNECT_COUNT, DEFAULT_CONNECT_COUNT);
            rules.CameraSpeed = PlayerPrefs.GetFloat(KEY_CAMERA_SPEED, DEFAULT_CAMERA_ROTATION_SPEED);
            rules.PointerSpeed = PlayerPrefs.GetFloat(KEY_POINTER_SPEED, DEFAULT_POINTER_SPEED);

            return rules;
        }

        /// <summary>
        /// PlayerPrefs をリセットする
        /// </summary>
        public void Delete()
        {
            PlayerPrefs.DeleteKey(KEY_PLAYER_COUNT);
            PlayerPrefs.DeleteKey(KEY_TURN_COUNT);
            PlayerPrefs.DeleteKey(KEY_LIMIT_TIME);
            PlayerPrefs.DeleteKey(KEY_BOARD_SIZE);
            PlayerPrefs.DeleteKey(KEY_CONNECT_COUNT);
            PlayerPrefs.DeleteKey(KEY_CAMERA_SPEED);
            PlayerPrefs.DeleteKey(KEY_POINTER_SPEED);

            // セーブ存在フラグも削除
            PlayerPrefs.DeleteKey(KEY_EXISTS);

            PlayerPrefs.Save();
        }
    }
}