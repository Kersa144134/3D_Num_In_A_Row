// ======================================================
// InputManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-09-24
// 更新日時 : 2025-12-08
// 概要     : 物理ゲームパッドおよびキーボード・マウス入力を統合管理
//            配列取得した InputMappingConfig による入力マッピングを切り替え
// ======================================================

using UnityEngine;
using InputSystem.Data;
using System;

namespace InputSystem.Manager
{
    /// <summary>
    /// 入力管理マスタークラス
    /// 物理ゲームパッドとキーボード・マウス入力を統合
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        // ======================================================
        // シングルトンインスタンス
        // ======================================================

        /// <summary>InputManager のインスタンス</summary>
        public static InputManager Instance { get; private set; }

        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("入力マッピング設定")]
        /// <summary>配列で取得</summary>
        [SerializeField] private InputMappingConfig[] _inputMappingConfigs;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>入力デバイス切替を管理するマネージャー</summary>
        private DeviceManager _deviceManager;

        /// <summary>ボタン状態を更新するサービス</summary>
        private ButtonStateUpdateService _buttonStateUpdateService = new ButtonStateUpdateService();

        /// <summary>スティック/D-Pad状態を管理するマネージャー</summary>
        private StickStateManager _stickStateManager = new StickStateManager();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// ボタン状態配列
        /// GamepadInputType の順序で固定
        /// </summary>
        private ButtonState[] _buttonStates;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>入力デバイス切替を管理するマネージャー</summary>
        public DeviceManager DeviceManager => _deviceManager;

        /// <summary>ボタンAの状態</summary>
        public ButtonState ButtonA => _buttonStates[(int)GamepadInputType.ButtonA];

        /// <summary>ボタンBの状態</summary>
        public ButtonState ButtonB => _buttonStates[(int)GamepadInputType.ButtonB];

        /// <summary>ボタンXの状態</summary>
        public ButtonState ButtonX => _buttonStates[(int)GamepadInputType.ButtonX];

        /// <summary>ボタンYの状態</summary>
        public ButtonState ButtonY => _buttonStates[(int)GamepadInputType.ButtonY];

        /// <summary>左ショルダーの状態</summary>
        public ButtonState LeftShoulder => _buttonStates[(int)GamepadInputType.LeftShoulder];

        /// <summary>右ショルダーの状態</summary>
        public ButtonState RightShoulder => _buttonStates[(int)GamepadInputType.RightShoulder];

        /// <summary>左トリガーの状態</summary>
        public ButtonState LeftTrigger => _buttonStates[(int)GamepadInputType.LeftTrigger];

        /// <summary>右トリガーの状態</summary>
        public ButtonState RightTrigger => _buttonStates[(int)GamepadInputType.RightTrigger];

        /// <summary>左スティックボタンの状態</summary>
        public ButtonState LeftStickButton => _buttonStates[(int)GamepadInputType.LeftStickButton];

        /// <summary>右スティックボタンの状態</summary>
        public ButtonState RightStickButton => _buttonStates[(int)GamepadInputType.RightStickButton];

        /// <summary>Startボタンの状態</summary>
        public ButtonState StartButton => _buttonStates[(int)GamepadInputType.Start];

        /// <summary>Selectボタンの状態</summary>
        public ButtonState SelectButton => _buttonStates[(int)GamepadInputType.Select];

        /// <summary>左スティックの入力ベクトル</summary>
        public Vector2 LeftStick => _stickStateManager.LeftStick;

        /// <summary>右スティックの入力ベクトル</summary>
        public Vector2 RightStick => _stickStateManager.RightStick;

        /// <summary>D-Pad の入力ベクトル</summary>
        public Vector2 DPad => _stickStateManager.DPad;

        /// <summary>現在適用中の入力マッピング配列のインデックス</summary>
        public int CurrentMappingIndex { get; private set; } = 0;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            // 入力マッピングが登録されていない場合の強制終了処理
            if (_inputMappingConfigs == null || _inputMappingConfigs.Length == 0)
            {
                Debug.LogError("[InputManager] InputMappingConfigs が設定されていません。");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _deviceManager = new DeviceManager(_inputMappingConfigs);

            // enum の数だけ配列を確保
            int enumLength = Enum.GetValues(typeof(GamepadInputType)).Length;

            _buttonStates = new ButtonState[enumLength];

            // 各ボタン状態を初期化
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                _buttonStates[i] = new ButtonState();
            }
        }

        private void Update()
        {
            if (Instance != this)
            {
                return;
            }

            // デバイス入力更新
            _deviceManager.UpdateDevices();

            // ボタン状態更新
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                GamepadInputType type = (GamepadInputType)i;

                _buttonStateUpdateService.UpdateButtonState(
                    _deviceManager.ActiveController,
                    type,
                    _buttonStates[i]
                );
            }

            // スティック状態更新
            _stickStateManager.UpdateStickStates(_deviceManager.ActiveController);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 外部から入力マッピングを設定する
        /// </summary>
        /// <param name="index">マッピング配列のインデックス</param>
        public void SetInputMapping(in int index)
        {
            if (_inputMappingConfigs == null || index < 0 || index >= _inputMappingConfigs.Length)
            {
                return;
            }

            _deviceManager.SetMapping(_inputMappingConfigs[index]);

            // 適用中のインデックスを更新
            CurrentMappingIndex = index;
        }

        /// <summary>
        /// 現在適用中の入力マッピングインデックスを取得する
        /// </summary>
        /// <returns>入力マッピング配列のインデックス</returns>
        public int GetCurrentMappingIndex()
        {
            return CurrentMappingIndex;
        }
    }
}