// ======================================================
// DeviceManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-12-08
// 概要     : 入力デバイスの更新・切替を管理するクラス
//            物理ゲームパッドと仮想ゲームパッドの切替を統一的に提供
// ======================================================

using UnityEngine;
using UnityEngine.InputSystem;
using InputSystem.Controller;
using InputSystem.Data;

namespace InputSystem.Manager
{
    /// <summary>
    /// 入力デバイスの更新・切替を管理するクラス
    /// 物理ゲームパッドが接続されていれば優先使用、未接続時は仮想ゲームパッドを使用
    /// </summary>
    public class DeviceManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>物理ゲームパッド用コントローラ</summary>
        private GamepadInputController _gamepadController;

        /// <summary>仮想ゲームパッド用コントローラ（キーボード＋マウス統合）</summary>
        private VirtualGamepadInputController _virtualController;

        /// <summary>入力マッピング設定配列</summary>
        private InputMappingConfig[] _mappingConfigs;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在アクティブな入力コントローラ</summary>
        public IGamepadInputSource ActiveController { get; private set; }

        /// <summary>物理ゲームパッドを使用している場合は true</summary>
        public bool UseGamepad { get; private set; }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// DeviceManager コンストラクタ
        /// 入力マッピング設定をもとに仮想コントローラを初期化
        /// </summary>
        /// <param name="mappingConfigs">入力マッピング設定配列</param>
        public DeviceManager(in InputMappingConfig[] mappingConfigs)
        {
            if (mappingConfigs == null || mappingConfigs.Length == 0)
            {
                return;
            }

            _mappingConfigs = mappingConfigs;

            // デフォルトは要素0のインゲーム用マッピング
            InitializeControllers(_mappingConfigs[0]);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 配列の指定インデックスのマッピング設定でコントローラを再初期化
        /// </summary>
        /// <param name="index">マッピング配列のインデックス</param>
        public void SetMapping(in InputMappingConfig mappingConfig)
        {
            if (mappingConfig == null)
            {
                return;
            }

            InitializeControllers(mappingConfig);
        }

        /// <summary>
        /// 接続状況に応じて入力デバイスを更新し、ActiveController を切替
        /// </summary>
        public void UpdateDevices()
        {
            // 物理ゲームパッドが接続されているか判定
            UseGamepad = Gamepad.current != null;

            if (UseGamepad)
            {
                _gamepadController.UpdateInputs();
                ActiveController = _gamepadController;
            }
            else
            {
                _virtualController.UpdateInputs();
                ActiveController = _virtualController;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定マッピングでコントローラを初期化
        /// </summary>
        /// <param name="mappingConfig">入力マッピング設定</param>
        private void InitializeControllers(in InputMappingConfig mappingConfig)
        {
            // 物理ゲームパッドコントローラ初期化
            _gamepadController = new GamepadInputController();

            // キーボード・マウスコントローラ初期化
            KeyboardInputController keyboard = new KeyboardInputController(mappingConfig.Mappings);
            MouseInputController mouse = new MouseInputController(mappingConfig.Mappings);

            // 仮想ゲームパッドコントローラ初期化
            _virtualController = new VirtualGamepadInputController(keyboard, mouse, mappingConfig.Mappings);

            // デフォルトでアクティブを設定
            ActiveController = UseGamepad ? _gamepadController : _virtualController;
        }
    }
}