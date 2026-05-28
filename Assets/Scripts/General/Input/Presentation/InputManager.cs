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
using InputSystem.Application;
using InputSystem.Domain;
using OptionSystem.Presentation;

namespace InputSystem.Presentation
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
        [SerializeField]
        private InputMappingConfig[] _inputMappingConfigs;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>入力デバイス切替を管理するサービス</summary>
        private DeviceSwitchService _deviceSwitchService;

        /// <summary>ボタン状態を更新するサービス</summary>
        private readonly ButtonStateUpdateService _buttonStateUpdateService = new ButtonStateUpdateService();

        /// <summary>スティック / D-Pad 状態を管理するサービス</summary>
        private readonly StickStateUpdateService _stickStateUpdateService = new StickStateUpdateService();

        /// <summary>ポインター状態を管理するサービス</summary>
        private readonly PointerStateUpdateService _pointerStateUpdateService = new PointerStateUpdateService();

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在適用中の入力マッピング配列のインデックス</summary>
        private int _currentMappingIndex = 0;

        /// <summary>
        /// ボタン状態配列
        /// GamepadInputType の順序で固定
        /// </summary>
        private ButtonState[] _buttonStates;

        /// <summary>左スティックの入力ベクトル</summary>
        private readonly StickState _leftStick = new StickState();

        /// <summary>右スティックの入力ベクトル</summary>
        private readonly StickState _rightStick = new StickState();

        /// <summary>D-Pad の入力ベクトル</summary>
        private readonly StickState _dPad = new StickState();

        /// <summary>ポインターの座標</summary>
        private Vector2 _pointer = Vector2.zero;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在適用中の入力マッピング配列のインデックス</summary>
        public int CurrentMappingIndex => _currentMappingIndex;

        /// <summary>ボタン A の状態</summary>
        public ButtonState ButtonA => _buttonStates[(int)GamepadInputType.ButtonA];

        /// <summary>ボタン B の状態</summary>
        public ButtonState ButtonB => _buttonStates[(int)GamepadInputType.ButtonB];

        /// <summary>ボタン X の状態</summary>
        public ButtonState ButtonX => _buttonStates[(int)GamepadInputType.ButtonX];

        /// <summary>ボタン Y の状態</summary>
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

        /// <summary>Start ボタンの状態</summary>
        public ButtonState StartButton => _buttonStates[(int)GamepadInputType.Start];

        /// <summary>Select ボタンの状態</summary>
        public ButtonState SelectButton => _buttonStates[(int)GamepadInputType.Select];

        /// <summary>左スティック</summary>
        public StickState LeftStick => _leftStick;

        /// <summary>右スティック</summary>
        public StickState RightStick => _rightStick;

        /// <summary>D-Pad</summary>
        public StickState DPad => _dPad;

        /// <summary>ポインターの座標</summary>
        public Vector2 Pointer => _pointer;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        /// <summary>現在アクティブな入力デバイス種別</summary>
        public IReadOnlyReactiveProperty<InputDeviceType> ActiveDeviceType =>
            _deviceSwitchService.ActiveDeviceType;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            // --------------------------------------------------
            // 入力マッピングが登録されていない場合の強制終了処理
            // --------------------------------------------------
            if (_inputMappingConfigs == null || _inputMappingConfigs.Length == 0)
            {
                Debug.LogError("[InputManager] InputMappingConfigs が設定されていません。");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit();
#endif
                return;
            }

            // --------------------------------------------------
            // インスタンス生成
            // --------------------------------------------------
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // --------------------------------------------------
            // カーソル非表示
            // --------------------------------------------------
            Cursor.visible = false;

            // --------------------------------------------------
            // クラス初期化
            // --------------------------------------------------
            _deviceSwitchService = new DeviceSwitchService(_inputMappingConfigs);

            // --------------------------------------------------
            // 入力状態初期化
            // --------------------------------------------------
            // ゲームパッド入力の種類だけ配列を確保
            int enumLength = Enum.GetValues(typeof(GamepadInputType)).Length;

            _buttonStates = new ButtonState[enumLength];

            // ボタン状態初期化
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                _buttonStates[i] = new ButtonState();
            }

            // ポインター初期位置設定
            _pointer = new Vector2(Screen.width / 2f, Screen.height / 2f);
        }

        private void Start()
        {
            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;

            if (_gameOptionManager == null)
            {
                Debug.LogError("[InputManager] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }
        }

        private void Update()
        {
            if (Instance != this)
            {
                return;
            }

            // デバイス入力更新
            _deviceSwitchService.UpdateDevices();

            // 現在のアクティブコントローラー
            IGamepadInputSource controller = _deviceSwitchService.ActiveController;

            // ボタン状態更新
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                GamepadInputType type = (GamepadInputType)i;

                _buttonStateUpdateService.UpdateButtonState(
                    controller,
                    type,
                    _buttonStates[i]
                );
            }

            // スティック状態更新
            _stickStateUpdateService.GetStickStates(
                controller,
                _leftStick,
                _rightStick,
                _dPad
            );

            // インゲームマッピングでないポインターロック中かつゲームパッド入力時は処理なし
            if (_currentMappingIndex != 0 && _deviceSwitchService.ActiveDeviceType.Value == InputDeviceType.Gamepad)
            {
                return;
            }

            // ポインター状態更新
            _pointerStateUpdateService.UpdatePointer(controller, ref _pointer, _gameOptionManager.PointerSpeed);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // イベント購読解除
            _disposables?.Dispose();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        /// <param name="mapping">入力マッピング変更ストリーム</param>
        /// <param name="pointerPosition">ポインター座標変更ストリーム</param>
        public void BindStreams(
            IObservable<int> mapping,
            IObservable<Vector2> pointerPosition)
        {
            mapping
                .DistinctUntilChanged()
                .Subscribe(index =>
                {
                    ApplyInputMapping(index);

                })
                .AddTo(_disposables);

            pointerPosition
                .DistinctUntilChanged()
                .Subscribe(position =>
                {
                    _pointerStateUpdateService.SetPointerPosition(
                        ref _pointer,
                        in position);

                })
                .AddTo(_disposables);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

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
            _currentMappingIndex = index;
        }
    }
}