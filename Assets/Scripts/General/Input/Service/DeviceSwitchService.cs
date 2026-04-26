// ======================================================
// DeviceSwitchService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2026-04-03
// 概要     : 入力デバイスの更新・切替を管理するサービス
// ======================================================

using InputSystem.Controller;
using InputSystem.Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem.Service
{
    /// <summary>
    /// 入力デバイスの更新・切替を管理するサービス
    /// </summary>
    public class DeviceSwitchService
    {
        // ======================================================
        // プライベートクラス
        // ======================================================

        /// <summary>
        /// マッピング単位のコントローラセット
        /// </summary>
        private class ControllerSet
        {
            /// <summary>物理ゲームパッドコントローラー</summary>
            public GamepadInputController Gamepad;

            /// <summary>仮想ゲームパッドコントローラー</summary>
            public VirtualGamepadInputController Virtual;
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>マッピングごとのコントローラキャッシュ配列</summary>
        private ControllerSet[] _controllerSets;

        /// <summary>現在使用中のコントローラセット</summary>
        private ControllerSet _currentSet;

        /// <summary>ゲームパッドの最終入力時間</summary>
        private float _lastGamepadInputTime;

        /// <summary>仮想パッドの最終入力時間</summary>
        private float _lastVirtualInputTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在アクティブな入力コントローラー</summary>
        public IGamepadInputSource ActiveController { get; private set; }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// 全マッピング分のコントローラを初期化してキャッシュする
        /// </summary>
        /// <param name="mappingConfigs">入力マッピング設定配列</param>
        public DeviceSwitchService(in InputMappingConfig[] mappingConfigs)
        {
            // マッピングが無効な場合は何もしない
            if (mappingConfigs == null || mappingConfigs.Length == 0)
            {
                return;
            }

            // 配列サイズをマッピング数で確保
            _controllerSets = new ControllerSet[mappingConfigs.Length];

            for (int i = 0; i < mappingConfigs.Length; i++)
            {
                InputMappingConfig mapping = mappingConfigs[i];
                ControllerSet set = new ControllerSet();

                // 物理ゲームパッド生成
                set.Gamepad = new GamepadInputController();

                // 仮想ゲームパッド生成
                KeyboardInputController keyboard = new KeyboardInputController(mapping.Mappings);
                MouseInputController mouse = new MouseInputController(mapping.Mappings);

                set.Virtual = new VirtualGamepadInputController(keyboard, mouse, mapping.Mappings);

                _controllerSets[i] = set;
            }

            // デフォルトは 0 番に設定
            _currentSet = _controllerSets[0];
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// マッピングインデックスを指定して切替
        /// </summary>
        /// <param name="index">マッピングインデックス</param>
        public void SetMapping(int index)
        {
            if (_controllerSets == null || index < 0 || index >= _controllerSets.Length)
            {
                return;
            }

            _currentSet = _controllerSets[index];
        }

        /// <summary>
        /// デバイス状態に応じてアクティブコントローラを更新
        /// </summary>
        public void UpdateDevices()
        {
            // --------------------------------------------------
            // 両方の入力を更新
            // --------------------------------------------------
            _currentSet.Gamepad.UpdateInputs();
            _currentSet.Virtual.UpdateInputs();

            // --------------------------------------------------
            // 入力検知 → 時間更新
            // --------------------------------------------------
            // ゲームパッド
            if (_currentSet.Gamepad.HasAnyInputThisFrame)
            {
                _lastGamepadInputTime = Time.unscaledTime;
            }

            // 仮想パッド
            if (_currentSet.Virtual.HasAnyInputThisFrame)
            {
                _lastVirtualInputTime = Time.unscaledTime;
            }

            // --------------------------------------------------
            // アクティブコントローラ決定
            // --------------------------------------------------
            // ゲームパッド
            if (_lastGamepadInputTime > _lastVirtualInputTime)
            {
                ActiveController = _currentSet.Gamepad;
                return;
            }

            // 仮想パッド
            if (_lastVirtualInputTime > _lastGamepadInputTime)
            {
                ActiveController = _currentSet.Virtual;
                return;
            }

            // 初期状態または同時の場合、ゲームパッドを優先
            ActiveController = _currentSet.Gamepad;
        }
    }
}