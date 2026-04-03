// ======================================================
// InputManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-09-24
// 更新日時 : 2025-12-08
// 概要     : 物理ゲームパッドおよびキーボード・マウス入力を統合管理
//            配列取得した InputMappingConfig による入力マッピングを切り替え
// ======================================================

using System;
using UnityEngine;
using UniRx;
using InputSystem.Data;
using InputSystem.Service;
using InputSystem.Controller;

namespace InputSystem
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
        /// <summary>入力マッピング配列</summary>
        [SerializeField] private InputMappingConfig[] _inputMappingConfigs;

        [Header("入力マッピング設定")]
        /// <summary>ポインター移動速度</summary>
        [SerializeField, Range(100f, 5000f)] private float _pointerSpeed = 1000f;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>入力デバイス切替を管理するマネージャー</summary>
        private DeviceSwitchService _deviceSwitchService;

        /// <summary>ボタン状態を更新するサービス</summary>
        private ButtonStateUpdateService _buttonStateUpdateService = new ButtonStateUpdateService();

        /// <summary>スティック/D-Pad状態を管理するマネージャー</summary>
        private StickStateUpdateService _stickStateUpdateService = new StickStateUpdateService();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// ボタン状態配列
        /// GamepadInputType の順序で固定
        /// </summary>
        private ButtonState[] _buttonStates;

        /// <summary>左スティックの入力ベクトル</summary>
        private Vector2 _leftStick = Vector2.zero;

        /// <summary>右スティックの入力ベクトル</summary>
        private Vector2 _rightStick = Vector2.zero;

        /// <summary>D-Pad の入力ベクトル</summary>
        private Vector2 _dPad = Vector2.zero;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>入力デバイス切替を管理するマネージャー</summary>
        public DeviceSwitchService DeviceSwitchService => _deviceSwitchService;

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
        public Vector2 LeftStick => _leftStick;

        /// <summary>右スティックの入力ベクトル</summary>
        public Vector2 RightStick => _rightStick;

        /// <summary>D-Pad の入力ベクトル</summary>
        public Vector2 DPad => _dPad;

        /// <summary>ポインターの座標</summary>
        public Vector2 Pointer { get; private set; } = Vector2.zero;

        /// <summary>現在適用中の入力マッピング配列のインデックス</summary>
        public int CurrentMappingIndex { get; private set; } = 0;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>マッピング変更購読</summary>
        private IDisposable _mappingSubscription;
        
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

            _deviceSwitchService = new DeviceSwitchService(_inputMappingConfigs);

            // enum の数だけ配列を確保
            int enumLength = Enum.GetValues(typeof(GamepadInputType)).Length;

            _buttonStates = new ButtonState[enumLength];

            // 各ボタン状態を初期化
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                _buttonStates[i] = new ButtonState();
            }

            // ポインター初期位置を画面中心に設定
            Pointer = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }

        private void Update()
        {
            if (Instance != this)
            {
                return;
            }

            // デバイス入力更新
            _deviceSwitchService.UpdateDevices();

            // ボタン状態更新
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                GamepadInputType type = (GamepadInputType)i;

                _buttonStateUpdateService.UpdateButtonState(
                    _deviceSwitchService.ActiveController,
                    type,
                    _buttonStates[i]
                );
            }

            // スティック状態更新
            _stickStateUpdateService.UpdateStickStates(
                _deviceSwitchService.ActiveController,
                ref _leftStick,
                ref _rightStick,
                ref _dPad
            );

            // ポインター状態更新
            UpdatePointer();
        }

        private void OnDestroy()
        {
            UnbindMappingStream();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力マッピング変更イベントを購読する
        /// </summary>
        public void BindMappingStream(IObservable<int> stream)
        {
            // 多重購読防止
            _mappingSubscription?.Dispose();

            _mappingSubscription = stream
                .Subscribe(index =>
                {
                    ApplyInputMapping(index);
                });
        }

        /// <summary>
        /// 入力マッピング変更イベントの購読を解除する
        /// </summary>
        public void UnbindMappingStream()
        {
            _mappingSubscription?.Dispose();
            _mappingSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// デバイスに応じてポインター座標を更新
        /// </summary>
        private void UpdatePointer()
        {
            if (_deviceSwitchService.ActiveController == null)
            {
                return;
            }

            // 仮想ゲームパッドなら絶対座標をそのまま適用
            if (_deviceSwitchService.ActiveController is VirtualGamepadInputController virtualController)
            {
                Pointer = virtualController.MousePosition;
            }
            // ゲームパッドなら右スティック入力を加算適用
            else if (_deviceSwitchService.ActiveController is GamepadInputController gamepadController)
            {
                Vector2 delta = gamepadController.RightStick * _pointerSpeed * Time.deltaTime;

                Pointer += delta;
            }

            // 画面外制限
            Pointer = new Vector2(
                Mathf.Clamp(Pointer.x, 0f, Screen.width),
                Mathf.Clamp(Pointer.y, 0f, Screen.height));
        }

        /// <summary>
        /// 入力マッピングを適用する
        /// </summary>
        private void ApplyInputMapping(in int index)
        {
            if (_inputMappingConfigs == null || index < 0 || index >= _inputMappingConfigs.Length)
            {
                return;
            }

            _deviceSwitchService.SetMapping(index);

            // 適用中のインデックスを更新
            CurrentMappingIndex = index;
        }
    }
}