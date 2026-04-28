// ======================================================
// CameraPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
// 更新日時 : 2026-04-20
// 概要     : カメラ入力・状態更新・描画反映を管理するプレゼンター
// ======================================================

using System;
using UnityEngine;
using UniRx;
using CameraSystem.Application;
using CameraSystem.Domain;
using OptionSystem.Presentation;
using InputSystem.Presentation;
using UpdateSystem.Domain;

namespace CameraSystem.Presentation
{
    /// <summary>
    /// カメラ制御用プレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.CameraPresenter)]
    public sealed class CameraPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("回転制限設定")]
        /// <summary>X 回転の最小値</summary>
        [SerializeField]
        private float _minRotationX = -90.0f;

        /// <summary>X 回転の最大値</summary>
        [SerializeField]
        private float _maxRotationX = 90.0f;

        [Header("投影設定")]
        /// <summary>透視時の視野角</summary>
        [SerializeField]
        private float _perspectiveFov = 60.0f;

        /// <summary>平行時のサイズ</summary>
        [SerializeField]
        private float _orthographicSize = 0.75f;

        /// <summary>NearClip</summary>
        [SerializeField]
        private float _nearClip = 0.1f;

        /// <summary>FarClip</summary>
        [SerializeField]
        private float _farClip = 100.0f;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>モデル</summary>
        private CameraModel _cameraModel;

        /// <summary>ビュー</summary>
        private CameraView _cameraView;

        /// <summary>角度計算ユーティリティ</summary>
        private readonly CameraAngleUtility _angleUtility = new CameraAngleUtility();

        /// <summary>投影補間サービス</summary>
        private CameraProjectionService _projectionService;

        /// <summary>回転ユースケース</summary>
        private CameraRotationUseCase _rotationUseCase;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>カメラ参照</summary>
        private Camera _camera;

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;
        
        /// <summary>回転の最大速度（度 / 秒）</summary>
        private float _maxRotationSpeed;

        /// <summary>回転の加速度（度 / 秒の2乗）</summary>
        private float _rotationAcceleration;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// 回転加速度の倍率
        /// </summary>
        private const float ROTATION_ACCELERATION_MULTIPLIER = 2f;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>入力ロック状態購読</summary>
        private IDisposable _inputLockSubscription;

        /// <summary>ボード回転準備状態購読</summary>
        private IDisposable _rotationPreparationSubscription;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;

            // カメラ取得
            _camera = Camera.main;

            if (_gameOptionManager == null || _inputManager == null || _camera == null)
            {
                Debug.LogError("[CameraPresenter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            // オプションを取得
            _maxRotationSpeed = _gameOptionManager.CameraRotationSpeed;
            _rotationAcceleration = _maxRotationSpeed * ROTATION_ACCELERATION_MULTIPLIER;

            // 現在の Transform の回転を取得する
            Vector3 euler = transform.rotation.eulerAngles;

            // X 回転を -180 ～ 180 に正規化する
            float initialRotationX = _angleUtility.NormalizeAngle(euler.x);

            // Y 回転を -180 ～ 180 に正規化する
            float initialRotationY = _angleUtility.NormalizeAngle(euler.y);

            // モデル、ビュー初期化
            _cameraModel = new CameraModel(
                initialRotationX,
                initialRotationY,
                _minRotationX,
                _maxRotationX
            );
            _cameraView = new CameraView(transform);

            // 初期化
            _projectionService = new CameraProjectionService(
                _perspectiveFov,
                _orthographicSize,
                _nearClip,
                _farClip
            );
            _rotationUseCase = new CameraRotationUseCase(
                _cameraModel,
                _maxRotationSpeed,
                _rotationAcceleration
            );
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // --------------------------------------------------
            // 入力取得
            // --------------------------------------------------
            // 左右入力を取得する
            float inputHorizontal = _inputManager.LeftStick.x;

            // 上下入力を取得する
            float inputVertical = _inputManager.LeftStick.y;

            // 入力値を Vector2 としてまとめる
            Vector2 input = new Vector2(inputHorizontal, inputVertical);

            // --------------------------------------------------
            // 回転処理
            // --------------------------------------------------
            // 入力がロックされている場合は処理なし
            if (_isInputLock)
            {
                return;
            }

            UpdateRotation(input, unscaledDeltaTime);
        }

        public void OnExit()
        {
            UnbindInputLockStream();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力ロック状態ストリームを購読する
        /// </summary>
        /// <param name="stream">true:ロック / false:解除</param>
        public void BindInputLockStream(in IObservable<bool> stream)
        {
            // 多重購読防止
            _inputLockSubscription?.Dispose();

            _inputLockSubscription = stream
                .Subscribe(isLock =>
                {
                    // 入力ロック状態を更新
                    _isInputLock = isLock;

                    if (_isInputLock)
                    {
                        // 回転速度リセット
                        _rotationUseCase.ResetRotationVelocity();
                    }
                });
        }

        /// <summary>
        /// 入力ロック状態ストリームの購読を解除する
        /// </summary>
        public void UnbindInputLockStream()
        {
            _inputLockSubscription?.Dispose();
            _inputLockSubscription = null;
        }

        /// <summary>
        /// ボード回転準備状態ストリームを購読する
        /// </summary>
        /// <param name="stream">true:開始 / false:終了</param>
        public void BindBoardRotationPreparationStream(in IObservable<Unit> stream)
        {
            // 多重購読防止
            _rotationPreparationSubscription?.Dispose();

            _rotationPreparationSubscription = stream
                .Subscribe(_ =>
                {
                    // 入力ロック
                    _isInputLock = true;

                    // ボード回転準備状態
                    _cameraModel.SetRotationX(90f);
                    _cameraModel.SetRotationY(0f);

                    _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
                });
        }

        /// <summary>
        /// ボード回転準備状態ストリームの購読を解除する
        /// </summary>
        public void UnbindBoardRotationPreparationStream()
        {
            _rotationPreparationSubscription?.Dispose();
            _rotationPreparationSubscription = null;
        }

        /// <summary>
        /// 投影方式を透視または平行に切り替える
        /// </summary>
        /// <param name="isPerspective">true:透視 / false:平行</param>
        public void SwitchProjection(in bool isPerspective)
        {
            _projectionService.SetProjection(_camera, isPerspective);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// カメラ回転処理
        /// </summary>
        /// <param name="unscaledDeltaTime">非スケールデルタ時間</param>
        private void UpdateRotation(in Vector2 input, in float unscaledDeltaTime)
        {
            // --------------------------------------------------
            // 回転更新
            // --------------------------------------------------
            _rotationUseCase.UpdateRotation(input, unscaledDeltaTime);

            // --------------------------------------------------
            // ビュー反映
            // --------------------------------------------------
            _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
        }
    }
}